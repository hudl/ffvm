using FFVM.Base.Utility;
using FFVM.Manager.Commands.BaseTypes;
using FFVM.Manager.Commands.BaseTypes.Attributes;

namespace FFVM.Manager.Commands.Image.Request;

public class InstallImageCommandRequest : IRepositorySelectable
{
    [CommandParameter("version", order: 1, description: "The version number for the FFmpeg/FFprobe utilities container to install (default latest).", isrequired: true)]
    public string? Version { get; set; }

    [CommandParameter("name", description: "A short name or alias for the version, name defaults to the ffmpeg version installed.")]
    public string? Name { get; set; }

    [RepositoryAliasCommandParameter]
    public string? RepositoryName { get; set; }
}
