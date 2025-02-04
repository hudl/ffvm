using FFVM.Base.Commands.BaseTypes;
using FFVM.Base.Commands.Request;
using FFVM.Base.IO.BaseTypes;
using FFVM.Base.Utility;

namespace FFVM.Base.Commands;

public class HelpCommandHandler(
    ProgramContext _context, 
    ILogger _logger, 
    IEnumerable<ICommandHandler> _commands)
{
    private HelpCommandRequest? _commandParameters;

    public static string CommandName => "help";
    public static string CommandDescription => "Displays the help and program usage menu.";
    public static Type? CommandParametersType => default;
    public bool IsValid()
    {
        _commandParameters = new HelpCommandRequest();
        CommandParameterUtility.ParseArgumentsIntoCommandParameters(_commandParameters, _context.Arguments);
        return _commandParameters != null;
    }
    public bool IsCommand() => _context.Command.Equals(CommandName, StringComparison.Ordinal);
    public async Task<int> Process()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_commandParameters?.CommandName))
            {
                return await DisplayMainHelp();
            }

            return await DisplayCommandHelp(); 
        }
        catch (Exception e)
        {
            _logger.WriteStdErr(e.Message, e);
            return ExitCodes.GeneralError;
        }

    }

    private Task<int> DisplayMainHelp()
    {
        var maximumCommandColumnWidth = _commands.Max(command => command.CommandName.Length);

        _logger.WriteStdOut($"");
        _logger.WriteStdOut($"Usage: {_context.ProgramName} COMMAND [OPTIONS]");
        _logger.WriteStdOut("");
        _logger.WriteStdOut("An FFmpeg/FFprobe version management, and emulator system.");
        _logger.WriteStdOut("");
        _logger.WriteStdOut("Available commands:");
        _logger.WriteStdOut($"   {CommandName.PadRight(maximumCommandColumnWidth)}  {CommandDescription}");

        foreach (var command in _commands)
        {
            _logger.WriteStdOut($"   {command.CommandName.PadRight(maximumCommandColumnWidth)}  {command.CommandDescription}");
        }
        
        _logger.WriteStdOut("");
        _logger.WriteStdOut($"Run '{_context.ProgramName} help COMMAND' for more information on a command ");

        return Task.FromResult(ExitCodes.Successful);
    }
    private Task<int> DisplayCommandHelp()
    {
        var matchingCommand = _commands.FirstOrDefault(command => command.CommandName.Equals(_commandParameters?.CommandName, StringComparison.Ordinal)); 
        if (matchingCommand == null)
        {
            _logger.WriteStdErr($"The command '{_commandParameters?.CommandName}' does not exist in the program.");
            return Task.FromResult(ExitCodes.GeneralError);
        }

        var availableParameters = CommandParameterUtility.GetPropertiesAndAttributes(matchingCommand.CommandParametersType!);
        var maximumParamaterNameWidth = availableParameters.Max(parameter => parameter.Value?.Attribute?.Name?.Length ?? 0);
        var orderedParameters = availableParameters
            .Where(parameter => parameter.Value?.Attribute?.Order > 0)
            .Select(parameter =>
            {
                var attributeRequiredIdentifier = parameter.Value?.Attribute?.IsRequired ?? true ? ":*" : "";
                return $"<{parameter.Value?.Attribute?.Name}{attributeRequiredIdentifier}>";
            }).ToList();
        var commandUsage = orderedParameters.Count != 0
                ? $"{_context.ProgramName} {matchingCommand.CommandName} {string.Join(' ', orderedParameters)} [OPTIONS]"
                : $"{_context.ProgramName} {matchingCommand.CommandName} [OPTIONS]";

        _logger.WriteStdOut($"");
        _logger.WriteStdOut($"Usage: {commandUsage}");
        _logger.WriteStdOut($"");
        _logger.WriteStdOut(matchingCommand.CommandDescription);
        _logger.WriteStdOut($"");
        _logger.WriteStdOut($"Available options:");
        foreach (var availableParameter in availableParameters)
        {
            _logger.WriteStdOut($"   --{availableParameter.Value.Attribute!.Name.PadRight(maximumParamaterNameWidth)}  {availableParameter.Value.Attribute.Description}");
        }
        
        return Task.FromResult(ExitCodes.Successful);
    }
}
