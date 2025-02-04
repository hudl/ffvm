using FFVM.Base.Utility;

namespace FFVM.Base.Commands.Request;

public class HelpCommandRequest
{
    [CommandParameter("command", order: 1, description: "Command name to provide additional detail on in help.")]
    public string? CommandName { get; set; }
}
