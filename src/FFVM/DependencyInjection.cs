using FFVM.Base.Commands;
using FFVM.Base.Config;
using FFVM.Base.Config.BaseTypes;
using FFVM.Base.IO;
using FFVM.Base.IO.BaseTypes;
using FFVM.Base.Shell;
using FFVM.Base.Shell.BaseTypes;
using Microsoft.Extensions.DependencyInjection;

namespace FFVM.Base;

public class DependencyInjection
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ProgramRunner>();
        services.AddSingleton<ProgramContext>();
        services.AddSingleton<HelpCommandHandler>();
        services.AddSingleton<ConfigurationProvider>();

        services.AddSingleton<ILogger, ConsoleLogger>();
        services.AddSingleton<IInputsProvider, ConsoleInputProvider>();
        services.AddSingleton<IConfigurationProvider, ConfigurationProvider>();
        services.AddSingleton<ISystemShellRunner, UniversalSystemShellRunner>();
        services.AddSingleton<IFFUtilityEmulator, FFUtilityEmulator>();
    }
}