using FFVM.Base.Config.BaseTypes;
using Newtonsoft.Json;

namespace FFVM.Base.Config;

public class ConfigurationProvider : IConfigurationProvider
{
    private readonly string _ffvmConfigUri = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ffvm");

    protected Configuration? _ffvmConfiguration;
    public Configuration? Configuration
    {
#pragma warning disable CS8766 // Nullability of reference types in return type doesn't match implicitly implemented member (possibly because of nullability attributes).
        get
#pragma warning restore CS8766 // Nullability of reference types in return type doesn't match implicitly implemented member (possibly because of nullability attributes).
        {
            if (_ffvmConfiguration == null)
            {
                if (File.Exists(_ffvmConfigUri))
                {
                    var _ffvmConfigurationRawContents = File.ReadAllText(_ffvmConfigUri);
                    _ffvmConfiguration = JsonConvert.DeserializeObject<Configuration>(_ffvmConfigurationRawContents);
                }
                else
                {
                    _ffvmConfiguration = new Configuration();
                    File.WriteAllText(_ffvmConfigUri, JsonConvert.SerializeObject(_ffvmConfiguration));
                }
            }

            return _ffvmConfiguration;

        }
    }

    public Task Save()
    {
        _ffvmConfiguration ??= new Configuration();

        var configurationString =  JsonConvert.SerializeObject(Configuration);
        
        File.WriteAllText(_ffvmConfigUri, configurationString);

        return Task.CompletedTask;
    }
}
