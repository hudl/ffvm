using FFVM.Base;
using FFVM.Base.Commands.BaseTypes;
using FFVM.Base.Config.BaseTypes;
using FFVM.Base.IO.BaseTypes;
using FFVM.Base.Utility;
using FFVM.Manager.Commands.Repository.Request;
using FFVM.Manager.Utility;

namespace FFVM.Manager.Commands.Repository;

public class ListRepositoryCommandHandler(
    ProgramContext _context, 
    ILogger _logger, 
    IConfigurationProvider _configurationProvider) : BaseCommandHandler<ListRepositoryCommandRequest>(_context, _logger, _privateCommandName, _privateCommandDescription), ICommandHandler
{
    private const string _privateCommandName = "list-repository";
    private const string _privateCommandDescription = "Lists the currently configured repositories with aliases.";

    public override Task<int> Process()
    {
        Guard.AgainstEmptyConfiguration(_configurationProvider);
        Guard.AgainstNull(Parameters, nameof(Parameters));

        if (_configurationProvider.Configuration.Repositories.Count == 0)
        {
            _logger.WriteStdErr("You currently have no installed repositories in your repository store.");
            return Task.FromResult(ExitCodes.Successful);
        }

        _logger.WriteStdErr("");

        var tableWriter = new StdErrTableWriter();
        tableWriter.AddHeader(" "); // default indicator
        tableWriter.AddHeader("NAME", _configurationProvider.Configuration.Repositories.Max(v => v.Name.Length));
        tableWriter.AddHeader("TYPE", _configurationProvider.Configuration.Repositories.Max(v => v.Type.ToString().Length));
        tableWriter.AddHeader("URL");

        var containerRepositories = _configurationProvider.Configuration.Repositories
            .OrderBy(v => v.IsDefault);

        foreach (var repository in containerRepositories)
        {
            var inUseIndicator = repository.IsDefault ? "*" : " ";
            tableWriter.AddRow(inUseIndicator, repository.Name, repository.Type.ToString(), repository.Url);
        }

        tableWriter.Write(_logger);

        return Task.FromResult(ExitCodes.Successful);
    }
}
