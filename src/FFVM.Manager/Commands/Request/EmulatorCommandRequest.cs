using FFVM.Base.Utility;
using FFVM.Manager.Models.Enums;

namespace FFVM.Manager.Commands.Request;

public class EmulatorCommandRequest
{
    [CommandParameter("onoff", order: 1, description: "A boolean indicating the need to turn off the ffvm emulator or not.", customFormatterType: typeof(EnumParameterFormatter<EmulatorStatusType>), isrequired: true)]
    public EmulatorStatusType Type { get; set; }
}
