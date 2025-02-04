namespace FFVM.Base.Shell.Enums;

public enum EmulatorFlags
{
    None = 0,
    EchoStdOut = 1 << 0,
    EchoStdErr = 1 << 1,
}