using FFVM.Base.Commands.BaseTypes;
using FFVM.Base.Config;
using FFVM.Base.Config.BaseTypes;
using FFVM.Base.IO.BaseTypes;
using FFVM.Manager.Services.BaseTypes;
using FFVM.Manager.Services.Requests;
using FFVM.Base.Utility;
using FFVM.Base.Exceptions;
using FFVM.Manager.Commands.Repository.Request;
using FFVM.Base;

namespace FFVM.Manager.Commands.Repository;

public class AddRepositoryCommandHandler(
    ProgramContext _context, 
    ILogger _logger,
    IConfigurationProvider _configurationProvider, 
    IFFUtilitiesSystemRepositoryServiceFactory _ffUtilitiesSystemRepositoryServiceFactory) : BaseCommandHandler<AddRepositoryCommandRequest>(_context, _logger, _privateCommandName, _privateCommandDescription), ICommandHandler
{
    private const string _privateCommandName = "add-repository";
    private const string _privateCommandDescription = "Adds a new container repository from which to pull for latest versions.";

    public override async Task<int> Process()
    {
        Guard.AgainstEmptyConfiguration(_configurationProvider);
        Guard.AgainstNull(Parameters, nameof(Parameters));
        Guard.AgainstNullOrWhitespace(Parameters?.Url, nameof(Parameters.Url));

        ValidateAndNormalizeInputRequestParameters();

        var matchingRepository = _configurationProvider.Configuration.Repositories.FirstOrDefault(r => r.Url == Parameters!.Url);
        if (matchingRepository != null)
        {
            throw new CommandWorkflowValidationException($"The requested repository '{Parameters!.Url}', already exists in the repository store");
        }

        var ffutilitiesRepositoryService = await _ffUtilitiesSystemRepositoryServiceFactory.GetFFUtilitiesSystemRepository(Parameters!.Type)
           ?? throw new FFUtilitiesRepositoryServiceNotFoundException(Parameters.Type);

        var validateRepositoryResult = await ffutilitiesRepositoryService.ValidateRepository(new ValidateRepositoryRequest
        {
            RepositoryUrl = Parameters.Url,
            AwsProfile = Parameters.AwsProfile,
            DockerHubUsername = Parameters.DockerHubUsername,
            DockerHubAccessToken = Parameters.DockerHubAccessToken,
        })
            ?? throw new CommandWorkflowValidationException($"Repository validation failed for the provided URL '{Parameters!.Url} ({Parameters!.Type})'");

        if (Parameters.MakeDefault)
        {
            _configurationProvider.Configuration.Repositories.ForEach(r => r.IsDefault = false);
        }

        _configurationProvider.Configuration.Repositories.Add(new ContainerRepository
        {
            Name = Parameters.Name!,
            Url = Parameters.Url!,
            IsDefault = Parameters.MakeDefault,
            AuthorizationId = Parameters.AwsProfile!,
            Type = ffutilitiesRepositoryService.RepositoryType
        });

        await _configurationProvider.Save();

        if (Parameters.Name?.Equals(Parameters.Url) ?? true)
        {
            _logger.WriteStdOut($"Successfully added repository '{Parameters!.Url}'.");
        }
        else
        {
            _logger.WriteStdOut($"Successfully added repository '{Parameters!.Url}' and tagged with name '{Parameters!.Name}'.");
        }

        return ExitCodes.Successful;
    }
    private void ValidateAndNormalizeInputRequestParameters()
    {
        Parameters!.Name = string.IsNullOrWhiteSpace(Parameters.Name) ? Parameters.Url : Parameters.Name;
        Parameters!.MakeDefault = Parameters.MakeDefault || _configurationProvider.Configuration.Repositories.Count == 0;

        if (Parameters.Type == ContainerRepositoryType.Unknown)
        {
            //assuming that amazon erc repository urls generally match the format <accountid>.dkr.ecr.<region>.amazonaws.com/<repository>
            if (Parameters.Url!.Contains(".ecr.", StringComparison.OrdinalIgnoreCase) && Parameters.Url.Contains("amazonaws.com", StringComparison.OrdinalIgnoreCase))
            {
                Parameters.Type = ContainerRepositoryType.AwsEcr;
                return;
            }

            //assuming that all docker hub URLs will not match ECR, but will also not contain an http(s) prefix 
            if (!Parameters.Url.Contains("http://", StringComparison.OrdinalIgnoreCase) && !Parameters.Url.Contains("https://", StringComparison.OrdinalIgnoreCase) && Parameters.Url.Contains('/', StringComparison.OrdinalIgnoreCase))
            {
                Parameters.Type = ContainerRepositoryType.DockerHub;
                return;
            }

            //by default we should assume unknown
            Parameters.Type = ContainerRepositoryType.Unknown;
        }
    }
}
