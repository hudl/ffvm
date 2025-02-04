using FFVM.Base;
using FFVM.Base.Commands.BaseTypes;
using FFVM.Base.Config.BaseTypes;
using FFVM.Base.Exceptions;
using FFVM.Base.IO.BaseTypes;
using FFVM.Base.Utility;
using FFVM.Manager.Commands.Repository.Request;

namespace FFVM.Manager.Commands.Repository;

public class RemoveRepositoryCommandHandler(
    ProgramContext _context, 
    ILogger _logger, 
    IConfigurationProvider _configurationProvider) : BaseCommandHandler<RemoveRepositoryCommandRequest>(_context, _logger, _privateCommandName, _privateCommandDescription), ICommandHandler
{
    private const string _privateCommandName = "remove-repository";
    private const string _privateCommandDescription = "Removes an existing container repository from which to pull for latest versions.";

    public override async Task<int> Process()
    {
        Guard.AgainstEmptyConfiguration(_configurationProvider);
        Guard.AgainstNull(Parameters, nameof(Parameters));
        Guard.AgainstNullOrWhitespace(Parameters?.RepositoryName, nameof(Parameters.RepositoryName));

        var matchingRepository = _configurationProvider.Configuration.Repositories.FirstOrDefault(r => r.Name.Equals(Parameters?.RepositoryName, StringComparison.OrdinalIgnoreCase))
            ?? throw new CommandWorkflowValidationException($"The requested repository '{Parameters?.RepositoryName}', does not exist in the repository store");

        _configurationProvider.Configuration.Repositories.RemoveAll(r => r.Name.Equals(Parameters?.RepositoryName, StringComparison.OrdinalIgnoreCase));

        _configurationProvider.Configuration.Versions.RemoveAll(v => v.RepositoryId.Equals(Parameters?.RepositoryName, StringComparison.OrdinalIgnoreCase));

        await _configurationProvider.Save();

        _logger.WriteStdOut($"Successfully removed repository '{Parameters?.RepositoryName}' and all attached images.");

        return ExitCodes.Successful;
    }
}
