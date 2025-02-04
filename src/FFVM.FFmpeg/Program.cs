using FFVM.Base;
using FFVM.Base.IO.BaseTypes;
using FFVM.Base.Shell.BaseTypes;
using FFVM.Base.Shell.Enums;
using FFVM.Base.Shell.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FFVM.FFmpeg;

internal class Program
{
    public const string ProgramName = "ffmpeg";
    static async Task Main(string[] args)
    {
        var hostBuilder = CreateHostBuilder(args).Build();
        var logger = hostBuilder.Services.GetRequiredService<ILogger>();
        var emulator = hostBuilder.Services.GetRequiredService<IFFUtilityEmulator>();

        try
        {
            var emulatorResults = await emulator.Run(new EmulatorRequest
            {
                ProgramName = ProgramName,
                Arguments = args,
                Flags = EmulatorFlags.EchoStdOut | EmulatorFlags.EchoStdErr
            });

            Environment.Exit(emulatorResults);
        }
        catch (Exception ex)
        {
            logger.WriteStdErr($"An error occured during emulation of {ProgramName}", ex);
            Environment.Exit(ExitCodes.GeneralError);
        }

    }

    private static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, builder) =>
            {
                builder.SetBasePath(Directory.GetCurrentDirectory());
            })
            .ConfigureServices((context, services) =>
            {
                //configure all base services and classes 
                DependencyInjection.ConfigureServices(services);

            });
    }
}