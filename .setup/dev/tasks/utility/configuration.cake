#load "./repo-metadata.cake"
#load "./process-helpers.cake"

using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Newtonsoft.Json;

public class CustomVariable
{ 
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;
    [JsonProperty("type")]
    public string Type { get; set; } = string.Empty;
    [JsonProperty("helpText")]
    public string HelpText { get; set; } = string.Empty;
    [JsonProperty("values")]
    public List<string> ValidValues { get; set; } = new List<string>();
    public string ActualValue { get; set; }
}

public class CustomRepository
{ 
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;
    [JsonProperty("auth")]
    public string Auth { get; set; } = string.Empty;
    [JsonProperty("url")]
    public string Url { get; set; } = string.Empty;
    [JsonProperty("default")]
    public bool IsDefault { get; set; } = false; 
}

public class CustomConfiguration
{
    [JsonProperty("sharedSso")]
    public string SharedSSOProfileName { get; set; } = string.Empty;
    [JsonProperty("defaultImage")]
    public string DefaultImage { get; set; } = string.Empty;
    [JsonProperty("images")]
    public List<string> Images { get; set; } = new List<string>(); 
    [JsonProperty("variables")]
    public List<CustomVariable> Variables { get; set; } = new List<CustomVariable>();
    [JsonProperty("repositories")]
    public List<CustomRepository> Repositories { get; set; } = new List<CustomRepository>();
}

#nullable enable
static class ConfigurationHelper
{ 
    public static CustomConfiguration? LoadCustomConfiguration() 
    { 
        var homePath =  RepoMetadata.Current.HomePath;
        var configurationPath = RepoMetadata.Current.ConfigurationScriptPath; 
        if (!System.IO.File.Exists(configurationPath)) 
        { 
            return null;
        }

        var configurationScriptText = System.IO.File.ReadAllText(configurationPath); 
        var customConfiguration = JsonConvert.DeserializeObject<CustomConfiguration>(configurationScriptText);

        if (customConfiguration == null) 
        { 
            throw new Exception("Malformed custom configuration script could not be loaded.");
        }
        if (customConfiguration.Variables.Count > 0) 
        { 
            ReadCustomVariables(customConfiguration, homePath);
        }

        return customConfiguration;
    }

    private static void ReadCustomVariables(CustomConfiguration? customConfiguration, string homePath) 
    { 
        foreach (var customVariable in customConfiguration!.Variables!)
        { 
            //read the custom variable
            switch (customVariable.Type) 
            { 
                case "aws_profile": 
                    GetAWSProfileFromConfiguration(customVariable, homePath);
                    break;
                default: 
                    throw new InvalidOperationException($"Custom variable type of '{customVariable.Type}' is not supported for variable '{customVariable.Name}'."); 
            }

            //replace instances of custom variable in images
            for (var i = 0; i < customConfiguration.Images.Count; i++)
            { 
                customConfiguration.Images[i] = customConfiguration.Images[i].Replace($"%{customVariable.Name}%", customVariable.ActualValue);
            }

            //replace instances of custom variable in repositories
            for (var i = 0; i < customConfiguration.Repositories.Count; i++)
            { 
                customConfiguration.Repositories[i].Name = customConfiguration.Repositories[i].Name.Replace($"%{customVariable.Name}%", customVariable.ActualValue);
                customConfiguration.Repositories[i].Auth = customConfiguration.Repositories[i].Auth.Replace($"%{customVariable.Name}%", customVariable.ActualValue);
                customConfiguration.Repositories[i].Url = customConfiguration.Repositories[i].Url.Replace($"%{customVariable.Name}%", customVariable.ActualValue);
            }
        }
    }
    private static void GetAWSProfileFromConfiguration(CustomVariable customVariable, string homePath) 
    { 
        var credentialsFile = new SharedCredentialsFile(System.IO.Path.Combine(homePath, ".aws", "credentials"));
        foreach (var validValue in customVariable.ValidValues) 
        { 
            if (credentialsFile.TryGetProfile(validValue, out _))
            {
                customVariable.ActualValue = validValue;
                return;
            }
        }

        var exceptionText = string.IsNullOrWhiteSpace(customVariable.HelpText) 
            ? $"AWS Profiles are not setup in default credentials file."
            : customVariable.HelpText; 
        throw new Exception(exceptionText);
    }
}
#nullable disable
