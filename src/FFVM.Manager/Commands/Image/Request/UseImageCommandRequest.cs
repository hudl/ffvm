using FFVM.Base.Utility;

namespace FFVM.Manager.Commands.Image.Request;


public class UseImageCommandRequest
{
    [CommandParameter("name", order: 1, description: "The version number for the FFmpeg/FFprobe utilities container to use (default latest).", isrequired: true)]
    public string? Name { get; set; }
}
