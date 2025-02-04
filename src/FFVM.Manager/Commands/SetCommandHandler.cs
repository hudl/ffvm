using FFVM.Base;
using FFVM.Base.Commands.BaseTypes;
using FFVM.Base.Config;
using FFVM.Base.Config.BaseTypes;
using FFVM.Base.Exceptions;
using FFVM.Base.IO.BaseTypes;
using FFVM.Base.Utility;
using FFVM.Manager.Commands.Request;

namespace FFVM.Manager.Commands;

public class SetCommandHandler(
    ProgramContext _context, 
    ILogger _logger, 
    IConfigurationProvider _configurationProvider) : BaseCommandHandler<SetInstallationCommandRequest>(_context, _logger, _privateCommandName, _privateCommandDescription), ICommandHandler
{
    private const string _privateCommandName = "set";
    private const string _privateCommandDescription = "Sets specific configuration properties for the ffvm emulator.";

    public override Task<int> Process()
    {
        Guard.AgainstNullOrWhitespace(Parameters?.SettingName, nameof(Parameters.SettingName));
        Guard.AgainstNullOrWhitespace(Parameters?.SettingValue, nameof(Parameters.SettingValue));

        var validSetConfigurationValues = SetConfigurationParameterUtility.GetPropertiesAndAttributes<Configuration>(); 
        if (validSetConfigurationValues.Count == 0)
        {
            throw new CommandWorkflowValidationException("Could not find any settable configuration values.");
        }           

        if (!validSetConfigurationValues.TryGetValue(Parameters?.SettingName ?? string.Empty, out var configurationParameter))
        {
            throw new CommandWorkflowValidationException($"The setting value '{Parameters?.SettingName}' does not exist in configuration.");
        }


        configurationParameter.PropertyInfo.SetValue(_configurationProvider.Configuration, Parameters?.SettingValue);

        _configurationProvider.Save();

        _logger.WriteStdErr($"Configuration value for '{configurationParameter.Attribute!.Name}' has been set to '{Parameters?.SettingValue}'");

        return Task.FromResult(ExitCodes.Successful);
    }

}
