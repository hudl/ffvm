using FFVM.Base.IO.Enums;

namespace FFVM.Base.IO.BaseTypes;

public interface ILogger
{
    void WriteStdOut(string message, LogSeverity logSeverity = LogSeverity.Info);
    void WriteStdErr(string message, LogSeverity logSeverity = LogSeverity.Info);
    void WriteStdErr(string message, Exception e);
}
