using FFVM.Base.Config.BaseTypes;
using FFVM.Base.IO.BaseTypes;
using FFVM.Base.Shell.BaseTypes;
using FFVM.Base.Shell.Models;
using FFVM.Manager.Utility.Extensions;
using System.Text.RegularExpressions;

namespace FFVM.Manager.Services.BaseTypes;

public abstract partial class BaseContainerRepositoryService
{
    private const string FFmpegMainVersionNameGroup = "ffmpegMainVersion";
    private const string FFmpegSubVersionNameGroup = "ffmpegSubVersion";
    private static readonly Regex _ffmpegVersionRegex = FFmpegVersionRegex();

    protected readonly ILogger _logger;
    protected readonly IInputsProvider _inputsProvider;
    protected readonly ISystemShellRunner _systemShellRunner;
    protected readonly IConfigurationProvider _configurationProvider;

    public BaseContainerRepositoryService(ILogger logger, IInputsProvider inputsProvider, ISystemShellRunner systemShellRunner, IConfigurationProvider configurationProvider)
    {
        _logger = logger;
        _inputsProvider = inputsProvider;
        _systemShellRunner = systemShellRunner;
        _configurationProvider = configurationProvider;
    }

    protected (string? ffmpegVersion, string? ffmpegPatch) PullImageAndGetFFMpegVersion(string imageUrl, bool echoPullCommand)
    {
        var dockerPullCommandOptions = echoPullCommand ? ShellCommandOptions.EchoStdErrOut() : ShellCommandOptions.NoEcho();
        var dockerPullResults = _systemShellRunner.RunCommand($"docker pull {imageUrl}", dockerPullCommandOptions);
        if (dockerPullResults.ExitCode > 0)
        {
            _logger.WriteStdErr($"The docker pull command failed, console output:");
            _logger.WriteStdErr(dockerPullResults.StdErr);
            return (null, null);
        }

        //execute the ffmpeg container and pull the ffmpeg version that is installed. 
        var ffmpegResults = _systemShellRunner.RunCommand($"docker run --rm --entrypoint ffmpeg {imageUrl} -version", ShellCommandOptions.NoEcho());

        var ffmpegPatch = string.Empty;
        var ffmpegVersion = string.Empty;
        _ffmpegVersionRegex
            .MatchExpression(ffmpegResults.StdOut)
            .GetNamedGroupValue(FFmpegMainVersionNameGroup, s => ffmpegVersion = s)
            .GetNamedGroupValue(FFmpegSubVersionNameGroup, s => ffmpegPatch = s);

        return (ffmpegVersion, ffmpegPatch);
    }

    [GeneratedRegex(@"ffmpeg version [A-z]?(?<ffmpegMainVersion>[A-z0-9\.]+)(?<ffmpegSubVersion>[A-z0-9\-\.]*?) Copyright")]
    private static partial Regex FFmpegVersionRegex();
}
