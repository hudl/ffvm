using System.Diagnostics;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using FFVM.Base;
using FFVM.Base.Config.BaseTypes;
using FFVM.Base.Exceptions;
using FFVM.Base.IO.BaseTypes;
using FFVM.Base.Utility;
using FFVM.Manager.Models;
using FFVM.Manager.Services.BaseTypes;
using FFVM.Manager.Services.Requests;
using FFVM.Manager.Services.Results;

namespace FFVM.Manager.Services;

public class AmazonEcrAuthenticationService(
    ProgramContext _context, 
    IConfigurationProvider _configurationProvider, 
    IInputsProvider _inputsProvider, 
    ILogger _logger) : IContainerAuthenicationService<AmazonEcrAuthentication, AmazonEcrCredentials>
{
    private const string FFVM_SSO_AWS_CREDENTIALS_PROFILE_NAME = "ffvm-sso";

    /*
    The steps for aws ecr authentication have many paths. A user can contain profiles with Secret Key 
    and Access Key, or they can use AWS SSO. In the event of SSO, some users have a centralized profile
    where updated SSO credentials are stored. 

        1. Determine if the provided profile are SSO based
        2. If SSO based:
            2.1. Check if the credentials are still valid
            2.2. If not valid, refresh the credentials
            2.3. Return the credentials
        3. If not SSO based, return the credentials
    */
    public async Task<LoginResult<AmazonEcrCredentials>?> Login(LoginRequest<AmazonEcrAuthentication> loginRequest)
    {
        Guard.AgainstEmptyConfiguration(_configurationProvider);
        Guard.AgainstNull(loginRequest, nameof(loginRequest));

        var awsProfileStore = GetProfileStore();
        var awsProfileName = loginRequest.Authentication?.AwsProfile ?? _inputsProvider.GetValue("Enter AWS Profile: ");

        var awsProfile = GetAWSProfile(awsProfileStore, awsProfileName)
            ?? throw new CommandWorkflowValidationException("The provided AWS profile does not exist in the AWS credentials store.");

        var awsCredentials = awsProfile.GetAWSCredentials(awsProfileStore)
            ?? throw new CommandWorkflowValidationException("The provided AWS profile does not contain valid credentials.");

        if (awsCredentials is SSOAWSCredentials)
        {
            return await LoginWorkflowSSO(awsProfileStore, awsProfileName);
        }

        var validationTime = DateTime.UtcNow;
        return new LoginResult<AmazonEcrCredentials>()
        {
            ExpiresAt = validationTime.AddHours(1),
            VerifiedAt = validationTime,
            CredentialsContainer = new(awsCredentials)
        };
    }
    private async Task<LoginResult<AmazonEcrCredentials>?> LoginWorkflowSSO(SharedCredentialsFile awsProfileStore, string awsProfileName)
    {
        //we need to determine if the configuration contains an SSO base profile from which to pull credentials 
        var sharedCredentialsProfileName = !string.IsNullOrWhiteSpace(_configurationProvider.Configuration?.SSOProfileName)
                ? _configurationProvider.Configuration.SSOProfileName
                : FFVM_SSO_AWS_CREDENTIALS_PROFILE_NAME;

        // First of all, check if the profile is already present
        var sharedCredentialsProfile = GetAWSProfile(awsProfileStore, sharedCredentialsProfileName);

        // If the profile is not present or the profile is present but credentials have expired,
        // credentials are not valid. We assume that, by default, they are not valid
        var areAWSCredentialsStillValid = false;
        if (sharedCredentialsProfile != null)
        {
            // If the shared profile is present, get the credentials associated with it
            var sharedCredentials = sharedCredentialsProfile.GetAWSCredentials(awsProfileStore);
            if (sharedCredentials != null)
            {
                // If we have credentials, check if they are still valid
                areAWSCredentialsStillValid = await AreAWSCredentialsStillValid(sharedCredentials, awsProfileName);
            }
        }

        // If we do not have valid credentials, we need to refresh them
        if (!areAWSCredentialsStillValid)
        {
            _logger.WriteStdOut($"Refreshing AWS IIC credentials stored at {GetHomeDirectory()} for profile {awsProfileName} now.");
            RefreshAWSCredentials(awsProfileStore, awsProfileName, sharedCredentialsProfileName);
        }

        var credentialProfileStoreChain = new CredentialProfileStoreChain();
        var credentialsExist = credentialProfileStoreChain.TryGetAWSCredentials(sharedCredentialsProfileName, out var awsCredentials);
        if (!credentialsExist)
        {
            throw new CommandWorkflowValidationException("No valid AWS credentials found, cannot start application using IIC credentials.");
        }

        var validationTime = DateTime.UtcNow;
        return new LoginResult<AmazonEcrCredentials>()
        {
            ExpiresAt = validationTime.AddHours(1),
            VerifiedAt = validationTime,
            CredentialsContainer = new(awsCredentials)
        };
    }

    private static string GetHomeDirectory() => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    private static SharedCredentialsFile GetProfileStore() => new(Path.Combine(GetHomeDirectory(), ".aws/credentials"));
    private static CredentialProfile GetAWSProfile(SharedCredentialsFile profileStore, string profileName)
    {
        profileStore.TryGetProfile(profileName, out CredentialProfile profile);
        return profile;
    }
    private async Task<bool> AreAWSCredentialsStillValid(AWSCredentials credentials, string requestedProfile)
    {
        var client = new AmazonSecurityTokenServiceClient(credentials);
        try
        {
            var caller = await client.GetCallerIdentityAsync(new GetCallerIdentityRequest());
            return caller.Arn.Contains(requestedProfile);
        }
        catch (AmazonSecurityTokenServiceException)
        {
            _logger.WriteStdErr($"AWS IIC Role credentials are not valid anymore");
            return false;
        }
    }
    private void RefreshAWSCredentials(SharedCredentialsFile profileStore, string profileName, string awsCredentialsBase)
    {
        var profile = GetAWSProfile(profileStore, profileName);

        // If profile is null, that means that the SSO profile is not
        // present and, therefore, AWS IIC is not properly configured
        if (profile == null)
        {
            _logger.WriteStdErr("AWS IIC is not properly configured. Please run `aws configure sso` to configure it.");
            throw new ArgumentNullException(nameof(profileName));
        }
        else
        {
            var credentials = profile.GetAWSCredentials(profileStore);
            var ssoCredentials = credentials as SSOAWSCredentials;

            ssoCredentials!.Options.ClientName = _context.ProgramName;
            ssoCredentials!.Options.SsoVerificationCallback = args =>
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = args.VerificationUriComplete,
                        UseShellExecute = true
                    });
                }
                catch (Exception)
                {
                    _logger.WriteStdErr("You must generate your auth token using `aws sso login`");
                    throw;
                }
            };

            var ssoStaticCredentials = ssoCredentials.GetCredentials();
            var accessKeyId = ssoStaticCredentials.AccessKey;
            var secretAccessKey = ssoStaticCredentials.SecretKey;
            var sessionToken = ssoStaticCredentials.Token;

            var newProfile = new CredentialProfile(awsCredentialsBase, new CredentialProfileOptions
            {
                AccessKey = accessKeyId,
                SecretKey = secretAccessKey,
                Token = sessionToken,
            });

            profileStore.RegisterProfile(newProfile);
        }
    }
}
