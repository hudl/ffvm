using FFVM.Base.Shell.Enums;

namespace FFVM.Base.Shell.Models;

public class EmulatorRequest
{
    public string ProgramName { get; set; } = string.Empty;
    public string[] Arguments { get; set; } = [];
    public EmulatorFlags Flags { get; set; }

}