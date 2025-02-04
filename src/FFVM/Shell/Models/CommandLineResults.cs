namespace FFVM.Base.Shell.Models;

public class CommandLineResults
{
    public string StdOut { get; set; } = string.Empty;
    public string StdErr { get; set; } = string.Empty;
    public int ExitCode { get; set; }
}
