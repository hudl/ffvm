using FFVM.Base.Commands;
using FFVM.Base.Commands.BaseTypes;
using FFVM.Base.Exceptions;
using FFVM.Base.IO.BaseTypes;
using FFVM.Base.IO.Enums;

namespace FFVM.Base;

public class ProgramRunner(
    ProgramContext _context, 
    HelpCommandHandler _helpCommand, 
    ILogger _logger, 
    IEnumerable<ICommandHandler> _commands)
{
    public async Task<int?> Execute()
    {
        try
        {
            if (_helpCommand.IsCommand() && _helpCommand.IsValid())
            {
                return await _helpCommand.Process();
            }

            var matchingCommand = _commands.FirstOrDefault(c => c.IsCommand());
            if (matchingCommand == null)
            {
                _logger.WriteStdErr($"Could not find a matching command for `{_context.Command}`, try again.", LogSeverity.Error);
                return ExitCodes.GeneralError;
            }

            if (!matchingCommand.IsValid())
            {
                _logger.WriteStdErr($"Command for `{_context.Command}` is not valid, check usage and try again.", LogSeverity.Error);
                return ExitCodes.GeneralError;
            }

            return await matchingCommand.Process();
        }
        catch (CommandWorkflowValidationException ex)
        {
            _logger.WriteStdErr(ex.Message);
            return ExitCodes.GeneralError;
        }
        catch (Exception ex)
        {
            _logger.WriteStdErr(ex.Message);
            if (!string.IsNullOrWhiteSpace(ex.StackTrace))
            {
                _logger.WriteStdErr(ex.StackTrace);
            }
            return ExitCodes.GeneralError;
        }
    }
}
