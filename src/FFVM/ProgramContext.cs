using FFVM.Base.IO.BaseTypes;
using FFVM.Base.IO.Enums;

namespace FFVM.Base;

public class ProgramContext(ILogger _logger)
{

    public string ProgramName { get; set; } = string.Empty;
    public string Command { get; set; } = string.Empty;
    public List<string> Arguments { get; set; } = [];
    public int ExitCode { get; set; } = ExitCodes.Successful;

    public bool TryInitializeFromArgs(string[] args, string programName)
    {
        if (args == null || args?.Length < 1)
        {
            _logger.WriteStdErr($"Arguments are invalid, require at least one argument for command context.", LogSeverity.Error);
            return false;
        }

        ProgramName = programName;
        Command = args?.FirstOrDefault() ?? string.Empty;
        Arguments = args?.Skip(1)?.ToList() ?? [];

        return true;
    }
}
