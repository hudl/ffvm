#load "./repo-metadata.cake"

using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;


static class ProcessHelpers
{
    private enum ProcessFlags
    {
        None = 0,
        RedirectStandardOutput = 1,
        RedirectStandardError = 2,
    }

    private class ProcessResults
    {
        public ProcessResults(string standardOutput, string standardError, int exitCode)
        {
            StandardOutput = standardOutput;
            StandardError = standardError;
            ExitCode = exitCode;
        }
        public string StandardOutput { get; set; }
        public string StandardError { get; set; }
        public int ExitCode { get; set; }
    }

    private static ProcessResults RunProcessBase(string applicationName, string arguments, ProcessFlags flags)
    {
        var workingDirectory = System.IO.Path.Combine(RepoMetadata.Current.RepoRootPath, ".setup", "build");

        var proc = new Process();
        proc.StartInfo.FileName = applicationName;
        proc.StartInfo.Arguments = arguments;
        proc.StartInfo.CreateNoWindow = true;
        proc.StartInfo.RedirectStandardError = flags.HasFlag(ProcessFlags.RedirectStandardError);
        proc.StartInfo.RedirectStandardOutput = flags.HasFlag(ProcessFlags.RedirectStandardOutput);
        proc.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
        proc.StartInfo.WorkingDirectory = workingDirectory;
        proc.StartInfo.UseShellExecute = false;

        proc.Start();
        proc.WaitForExit();

        var standardOutput = flags.HasFlag(ProcessFlags.RedirectStandardOutput) ? proc.StandardOutput.ReadToEnd() : string.Empty;
        var standardError = flags.HasFlag(ProcessFlags.RedirectStandardError) ? proc.StandardError.ReadToEnd() : string.Empty;

        return new ProcessResults(standardOutput, standardError, proc.ExitCode);
    }
    public static void RunProcess(string applicationName, string arguments) => RunProcessBase(applicationName, arguments, ProcessFlags.None);
    public static string RunProcessWithStdOut(string applicationName, string arguments) => RunProcessBase(applicationName, arguments, ProcessFlags.RedirectStandardOutput).StandardOutput;

    public static void RunWithFailure(string applicationName, string arguments, string failureMessage)
    {
        var result = RunProcessBase(applicationName, arguments, ProcessFlags.None);
        if (result.ExitCode != 0)
        {
            throw new Exception(failureMessage);
        }
    }
}