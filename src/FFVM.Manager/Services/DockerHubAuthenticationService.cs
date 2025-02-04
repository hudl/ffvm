using System.Runtime.InteropServices;
using FFVM.Base;
using FFVM.Base.Config;
using FFVM.Base.Config.BaseTypes;
using FFVM.Base.IO.BaseTypes;
using FFVM.Base.Shell.BaseTypes;
using FFVM.Base.Shell.Models;
using FFVM.Base.Utility;
using FFVM.Manager.Models;
using FFVM.Manager.Services.BaseTypes;
using FFVM.Manager.Services.Requests;
using FFVM.Manager.Services.Results;

namespace FFVM.Manager.Services;

public class DockerHubAuthenticationService(
    IConfigurationProvider _configurationProvider, 
    IInputsProvider _inputsProvider, 
    ISystemShellRunner _systemShellRunner, 
    IDockerHubApiService _dockerHubApiService) : IContainerAuthenicationService<DockerHubAuthentication, DockerHubCredentials>
{
    private const string DockerHubAuthorizationRecordKey = "docker";
    private const int DockerHubCredentialsExpirationHours = 12;

    public async Task<LoginResult<DockerHubCredentials>?> Login(LoginRequest<DockerHubAuthentication> loginRequest)
    {
        Guard.AgainstEmptyConfiguration(_configurationProvider);
        Guard.AgainstNull(loginRequest, nameof(loginRequest));

        var currentTime = DateTime.UtcNow;
        var matchingAuthorizationRecord = _configurationProvider.Configuration.Authorizations.FirstOrDefault(a => a.Id == DockerHubAuthorizationRecordKey);
        if (matchingAuthorizationRecord?.ExpiresAt != null && matchingAuthorizationRecord?.ExpiresAt > currentTime.AddHours(1))
        {
            return new LoginResult<DockerHubCredentials> { ExpiresAt = matchingAuthorizationRecord.ExpiresAt, VerifiedAt = currentTime, CredentialsContainer = new(matchingAuthorizationRecord.Token!) };
        }

        var username = loginRequest.Authentication?.Username ?? _inputsProvider.GetValue("Enter DockerHub Username: ");
        var accessToken = loginRequest.Authentication?.PasswordOrToken ?? _inputsProvider.GetSecretValue("Enter DockerHub AccessToken: ");
        var accessTokenPtr = Marshal.SecureStringToBSTR(accessToken);
        var decodedString = Marshal.PtrToStringBSTR(accessTokenPtr);
        Marshal.ZeroFreeBSTR(accessTokenPtr);

        var dockerLoginResults = _systemShellRunner.RunCommand($"docker login --username {username} --password-stdin", ShellCommandOptions.NoEcho(decodedString));
        if (dockerLoginResults.ExitCode != 0)
        {
            throw new Exception($"Failed to login to Docker with username {username}");
        }

        var dockerHubAuthenticationToken = await _dockerHubApiService.GetAuthenticationToken(new GetAuthenticationTokenRequest(username, accessToken));
        if (string.IsNullOrWhiteSpace(dockerHubAuthenticationToken))
        {
            throw new Exception($"Failed to get Docker Hub authentication token for username {username}");
        }

        if (matchingAuthorizationRecord == null)
        {
            matchingAuthorizationRecord = new AuthorizationRecord(DockerHubAuthorizationRecordKey, currentTime.AddHours(DockerHubCredentialsExpirationHours), dockerHubAuthenticationToken);
            _configurationProvider.Configuration.Authorizations.Add(matchingAuthorizationRecord);
        }
        else
        {
            matchingAuthorizationRecord.Token = dockerHubAuthenticationToken;
            matchingAuthorizationRecord.ExpiresAt = currentTime.AddHours(DockerHubCredentialsExpirationHours);
        }

        await _configurationProvider.Save();

        return new LoginResult<DockerHubCredentials> { ExpiresAt = matchingAuthorizationRecord.ExpiresAt, VerifiedAt = currentTime, CredentialsContainer = new(dockerHubAuthenticationToken) };
    }
}
