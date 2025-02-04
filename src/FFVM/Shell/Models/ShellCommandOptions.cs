namespace FFVM.Base.Shell.Models;

public class ShellCommandOptions(string? stdInData, bool redirectStdErr = true, bool redirectStdOut = true)
{
    public bool RedirectStdErr { get; set; } = redirectStdErr;
    public bool RedirectStdOut { get; set; } = redirectStdOut;
    public bool RedirectStdIn { get; set; } = !string.IsNullOrWhiteSpace(stdInData);
    public bool EchoStdError { get; set; } = false;
    public bool EchoStdOutput { get; set; } = false;
    public string? StdInData { get; set; } = stdInData; 

    public static ShellCommandOptions NoRedirect(string? stdInData = null) => new(stdInData, redirectStdErr: false, redirectStdOut: false);
    public static ShellCommandOptions NoEcho(string? stdInData = null) => new(stdInData);
    public static ShellCommandOptions EchoStdErr(string? stdInData = null) => new(stdInData) { EchoStdError = true };
    public static ShellCommandOptions EchoStdOut(string? stdInData = null) => new(stdInData) { EchoStdOutput = true };
    public static ShellCommandOptions EchoStdErrOut(string? stdInData = null) => new(stdInData) { EchoStdOutput = true, EchoStdError = true };
}
