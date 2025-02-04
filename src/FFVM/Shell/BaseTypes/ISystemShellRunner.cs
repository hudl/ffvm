using FFVM.Base.Shell.Models;

namespace FFVM.Base.Shell.BaseTypes;

public interface ISystemShellRunner
{
    bool ExecutesSuccessfully(string commandToRun, ShellCommandOptions options);
    CommandLineResults RunCommand(string commandToRun, ShellCommandOptions options);
}
