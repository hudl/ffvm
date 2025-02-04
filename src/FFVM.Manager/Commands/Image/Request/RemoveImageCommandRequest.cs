using FFVM.Base.Utility;

namespace FFVM.Manager.Commands.Image.Request;

public class RemoveImageCommandRequest
{
    [CommandParameter("name", order: 1, description: "The name for the image that you wish to uninstall.", isrequired: true)]
    public string? Name { get; set; }
}
