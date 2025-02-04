using FFVM.Base;
using FFVM.Base.Commands.BaseTypes;
using FFVM.Base.Config.BaseTypes;
using FFVM.Base.Exceptions;
using FFVM.Base.IO.BaseTypes;
using FFVM.Base.Shell.BaseTypes;
using FFVM.Base.Shell.Models;
using FFVM.Base.Utility;
using FFVM.Manager.Commands.Request;
using System.Runtime.InteropServices;

namespace FFVM.Manager.Commands;

public class EmulatorCommandHandler(
    ProgramContext _context, 
    ILogger _logger, 
    IConfigurationProvider _configurationProvider, 
    ISystemShellRunner _systemShellRunner) : BaseCommandHandler<EmulatorCommandRequest>(_context, _logger, _privateCommandName, _privateCommandDescription), ICommandHandler
{
    private const string FFmpegEmulator = "ffmpeg";
    private const string FFprobeEmulator = "ffprobe";

    private const string _privateCommandName = "emulator";
    private const string _privateCommandDescription = "Turns the emulator on/off to pass through ffmpeg to static resources.";

    private static void MakeEmulatorVisibleAs(string destinationFilePath, string sourceFilePath)
    {
        //if we have a file already at our destination location, and at our source, we should delete the destination file.
        if (File.Exists(sourceFilePath) && File.Exists(destinationFilePath)) 
        {
            File.Delete(destinationFilePath);
        }

        //this will move the file from one location or another
        if (File.Exists(sourceFilePath))
        {
            File.Copy(sourceFilePath, destinationFilePath, true);
        }
    }

    public override Task<int> Process()
    {
        Guard.AgainstEmptyConfiguration(_configurationProvider);

        //pull the current directory and find the ffprobe and ffmpeg executables 
        var currentExecutingDirectory = _configurationProvider.Configuration.InstallationPath;

        var fileExtension = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".exe" : string.Empty;
        var ffmpegSymUri = Path.Combine(currentExecutingDirectory, $"{FFmpegEmulator}{fileExtension}");
        var ffprobeSymUri = Path.Combine(currentExecutingDirectory, $"{FFprobeEmulator}{fileExtension}");
        var ffmpegEmulatorUri = Path.Combine(currentExecutingDirectory, $"{FFmpegEmulator}{fileExtension}.ffvm");
        var ffprobeEmulatorUri = Path.Combine(currentExecutingDirectory, $"{FFprobeEmulator}{fileExtension}.ffvm");
        var ffmpegNativeUri = Path.Combine(currentExecutingDirectory, $"{FFmpegEmulator}{fileExtension}.native");
        var ffprobeNativeUri = Path.Combine(currentExecutingDirectory, $"{FFprobeEmulator}{fileExtension}.native");

        _logger.WriteStdErr($"Setting emulator status to '{Parameters?.Type}'");

        if (Parameters?.Type == Models.Enums.EmulatorStatusType.On)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var commandSuccessful =
                    _systemShellRunner.ExecutesSuccessfully($"ln -sf \"{ffmpegEmulatorUri}\" \"{ffmpegSymUri}\"", ShellCommandOptions.NoEcho())
                 && _systemShellRunner.ExecutesSuccessfully($"ln -sf \"{ffprobeEmulatorUri}\" \"{ffprobeSymUri}\"", ShellCommandOptions.NoEcho());
                if (!commandSuccessful)
                {
                    throw new CommandWorkflowValidationException("Failed to fix symbolic links, try re-installing your ffvm.");
                }
            }
            else
            {
                MakeEmulatorVisibleAs(ffmpegEmulatorUri, ffmpegSymUri);
                MakeEmulatorVisibleAs(ffprobeEmulatorUri, ffprobeSymUri);
            }
        }
        else
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && (File.Exists(ffmpegNativeUri) || File.Exists(ffprobeNativeUri)))
            {
                var commandSuccessful = true;
                if (File.Exists(ffmpegNativeUri)) commandSuccessful = commandSuccessful && _systemShellRunner.ExecutesSuccessfully($"ln -sf \"{ffmpegNativeUri}\" \"{ffmpegSymUri}\"", ShellCommandOptions.NoEcho()); 
                if (File.Exists(ffprobeNativeUri)) commandSuccessful = commandSuccessful && _systemShellRunner.ExecutesSuccessfully($"ln -sf \"{ffprobeNativeUri}\" \"{ffprobeSymUri}\"", ShellCommandOptions.NoEcho());
                if (!commandSuccessful)
                {
                    throw new CommandWorkflowValidationException("Failed to fix symbolic links, try re-installing your ffvm.");
                }
            }
            else
            {
                File.Delete(ffmpegSymUri);
                File.Delete(ffprobeSymUri);
            }
        }

        return Task.FromResult(ExitCodes.Successful);
    }

}
