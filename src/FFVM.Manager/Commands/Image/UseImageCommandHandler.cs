using FFVM.Base;
using FFVM.Base.Commands.BaseTypes;
using FFVM.Base.Config.BaseTypes;
using FFVM.Base.Exceptions;
using FFVM.Base.IO.BaseTypes;
using FFVM.Base.Utility;
using FFVM.Manager.Commands.Image.Request;

namespace FFVM.Manager.Commands.Image;

public class UseImageCommandHandler(
    ProgramContext _context, 
    ILogger _logger, 
    IConfigurationProvider _configurationProvider) : BaseCommandHandler<UseImageCommandRequest>(_context, _logger, _privateCommandName, _privateCommandDescription), ICommandHandler
{
    private const string _privateCommandName = "use";
    private const string _privateCommandDescription = "Sets the currently in use version of FFmpeg/FFprobe utilities container.";

    public override async Task<int> Process()
    {
        Guard.AgainstEmptyConfiguration(_configurationProvider);
        Guard.AgainstNull(Parameters, nameof(Parameters));
        Guard.AgainstNullOrWhitespace(Parameters?.Name, nameof(Parameters.Name));

        //read & pull current configuration
        var matchingVersion = _configurationProvider.Configuration.Versions.FirstOrDefault(v => 
            v.Name.Equals(Parameters?.Name, StringComparison.OrdinalIgnoreCase) ||
            v.FFmpegVersion.Equals(Parameters?.Name, StringComparison.OrdinalIgnoreCase))
                ?? throw new CommandWorkflowValidationException($"The requested version '{Parameters?.Name}' is not installed.");

        _configurationProvider.Configuration.Versions.ForEach(v => v.IsSelected = false);
        matchingVersion.IsSelected = true;

        await _configurationProvider.Save();

        _logger.WriteStdOut($"Successfully set active image version to '{matchingVersion.Name}', ffmpeg version '{matchingVersion.FFmpegVersion}'.");

        return ExitCodes.Successful;
    }
}
