using FFVM.Base;
using FFVM.Base.Commands.BaseTypes;
using FFVM.Base.Config.BaseTypes;
using FFVM.Base.Exceptions;
using FFVM.Base.IO.BaseTypes;
using FFVM.Manager.Commands.Repository.Request;

namespace FFVM.Manager.Commands.Repository;

public class SetRepositoryCommandHandler(
    ProgramContext _context, 
    ILogger _logger, 
    IConfigurationProvider _configurationProvider) : BaseCommandHandler<SetRepositoryCommandRequest>(_context, _logger, _privateCommandName, _privateCommandDescription), ICommandHandler
{
    private const string _privateCommandName = "set-repository";
    private const string _privateCommandDescription = "Sets a configured repository to default from which to pull for latest versions.";

    public override async Task<int> Process()
    {
        var matchingRepository = _configurationProvider.Configuration.Repositories.FirstOrDefault(v => v.Name.Equals(Parameters?.RepositoryName, StringComparison.OrdinalIgnoreCase))
            ?? throw new CommandWorkflowValidationException($"The requested repository '{Parameters?.RepositoryName}' is not installed.");

        _configurationProvider.Configuration.Repositories.ForEach(v => v.IsDefault = false);

        matchingRepository.IsDefault = true;

        await _configurationProvider.Save();

        _logger.WriteStdOut($"Successfully set default repository to '{Parameters?.RepositoryName}'.");

        return ExitCodes.Successful;
    }
}
