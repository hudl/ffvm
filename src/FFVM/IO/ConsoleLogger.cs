using FFVM.Base.IO.BaseTypes;
using FFVM.Base.IO.Enums;

namespace FFVM.Base.IO;

public class ConsoleLogger : ILogger
{
    private static readonly Dictionary<LogSeverity, ConsoleColor> _consoleColorMap = new()
    {
        { LogSeverity.Info, ConsoleColor.White },
        { LogSeverity.Warning, ConsoleColor.Yellow },
        { LogSeverity.Error, ConsoleColor.Red },
    };

    public void WriteStdOut(string message, LogSeverity logSeverity = LogSeverity.Info) => Write(Console.Out, message, _consoleColorMap[logSeverity]);
    public void WriteStdErr(string message, LogSeverity logSeverity = LogSeverity.Info) => Write(Console.Out, message, _consoleColorMap[logSeverity]);
    public void WriteStdErr(string message, Exception e)
    {
        Write(Console.Error, message, ConsoleColor.Red);
        Write(Console.Error, $"{nameof(Exception)}: {e.Message}", ConsoleColor.Red);
        Write(Console.Error, $"{nameof(e.StackTrace)}: {e.StackTrace}", ConsoleColor.Red);
    }
    private static void Write(TextWriter streamWriter, string message, ConsoleColor color)
    {
        var previousColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        streamWriter.WriteLine(message);
        Console.ForegroundColor = previousColor;
    }
}
