using FFVM.Base.Utility;

namespace FFVM.Manager.Commands.Request;

public class SetInstallationCommandRequest
{
    [CommandParameter("name", order: 1, description: "The name of the configuration setting to change.", isrequired: true)]
    public string? SettingName { get; set; }

    [CommandParameter("value", order: 2, description: "The value for the configuration setting to change.", isrequired: true)]
    public string? SettingValue { get; set; }
}
