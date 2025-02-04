using FFVM.Base.IO.BaseTypes;
using FFVM.Base.IO.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace FFVM.Base;

public class ProgramMain
{
    public static async Task Main(IServiceProvider serviceProvider, string programName, string[] args)
    {
        var logger = serviceProvider.GetRequiredService<ILogger>();
        var context = serviceProvider.GetRequiredService<ProgramContext>();

        if (!context?.TryInitializeFromArgs(args, programName) ?? true)
        {
            logger.WriteStdErr($"Could not initialize a program context, refer to program usage.", LogSeverity.Error);
            Environment.Exit(ExitCodes.GeneralError);
        }

        var returnCode = await serviceProvider?.GetService<ProgramRunner>()?.Execute()! ?? ExitCodes.GeneralError;
        if (returnCode > 0)
        {
            logger.WriteStdErr($"Program:{programName} did not exit successfully.", LogSeverity.Error);
            Environment.Exit(returnCode);
        }

        Environment.Exit(ExitCodes.Successful);
    }
}
