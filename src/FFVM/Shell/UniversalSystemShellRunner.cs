using FFVM.Base.IO.BaseTypes;
using FFVM.Base.Shell.BaseTypes;
using FFVM.Base.Shell.Models;
using System.Diagnostics;
using System.Text;

namespace FFVM.Base.Shell;

public class UniversalSystemShellRunner(ILogger _logger) : ISystemShellRunner
{
    public bool ExecutesSuccessfully(string commandToRun, ShellCommandOptions options) => RunCommand(BreakdownCommand(commandToRun), options).ExitCode == 0;
    public bool ExecutesSuccessfully(string programName, string programArgs, ShellCommandOptions options) => RunCommand(programName, programArgs, options).ExitCode == 0;
    public CommandLineResults RunCommand(string commandToRun, ShellCommandOptions options) => RunCommand(BreakdownCommand(commandToRun), options);
    public CommandLineResults RunCommand((string programName, string programArgs) programTuple, ShellCommandOptions options) => RunCommand(programTuple.programName, programTuple.programArgs, options);
    public CommandLineResults RunCommand(string programName, string programArgs, ShellCommandOptions options)
    {
        try
        {
            var stdErr = new StringBuilder();
            var stdOut = new StringBuilder();
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = programName,
                Arguments = programArgs,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = options.RedirectStdErr,
                RedirectStandardOutput = options.RedirectStdOut,
                RedirectStandardInput = options.RedirectStdIn,
            };

            process.OutputDataReceived += (sender, e) =>
            {
                var data = e?.Data ?? string.Empty;
                stdOut.AppendLine(data);
                if (options.EchoStdOutput)
                {
                    _logger.WriteStdOut(data);
                }
            };
            process.ErrorDataReceived += (sender, e) =>
            {
                var data = e?.Data ?? string.Empty;
                stdErr.AppendLine(data);
                if (options.EchoStdOutput)
                {
                    _logger.WriteStdErr(data);
                }
            };
            process.Start();

            if (options.RedirectStdIn)
            {
                Task.Run(() =>
                {
                    process.StandardInput.Write(options.StdInData);
                    process.StandardInput.Close();
                });
            }

            if (options.RedirectStdErr)
            {
                process.BeginErrorReadLine();
            }
            if (options.RedirectStdOut)
            {
                process.BeginOutputReadLine();
            }
            process.WaitForExit();
            return new CommandLineResults
            {
                StdErr = stdErr.ToString(),
                StdOut = stdOut.ToString(),
                ExitCode = process.ExitCode
            };
        }
        catch (Exception e)
        {
            return new CommandLineResults
            {
                ExitCode = -1,
                StdErr = $"There was a problem running the command {e.Message}",
                StdOut = $"There was a problem running the command {e.Message}",
            };
        }
    }

    private static (string, string) BreakdownCommand(string commandToRun)
    {
        var commandParts = commandToRun.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
        var programName = commandParts.First();
        var programArgs = commandParts.Count > 1
            ? string.Join(' ', commandParts.Skip(1).ToArray())
            : string.Empty;
        return (programName, programArgs);
    }
}
