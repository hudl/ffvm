using FFVM.Base.Utility;

namespace FFVM.Manager.Models.Enums;

public enum ListCommandType
{
    [Mapping("installed")]
    Installed,
    [Mapping("available")]
    Available, 
}
