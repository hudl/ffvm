using FFVM.Base.Config;
using FFVM.Base.Config.BaseTypes;
using System.Runtime.CompilerServices;
using System.Security;

namespace FFVM.Base.Utility;

public class Guard
{
    public static void AgainstValuesOtherThan<T>(T objectValue, T requiredValue, string objectName)
    {
        if (!objectValue?.Equals(requiredValue) ?? true)
        {
            throw new($"Value of '{objectValue}' was given, value of '{requiredValue}' was expected for parameter '{objectName}'.");
        }
    }
    public static void AgainstNull<T>(T? objectValue, string objectName)
    {
        if (objectValue == null)
        {
            throw new ArgumentException($"Value of '{objectName}' cannot be null.");
        }
    }
    public static void AgainstEmptySecure(SecureString objectValue, string objectName)
    {
        if (objectValue?.Length == 0)
        {
            throw new ArgumentException($"Value of '{objectName}' cannot be null or empty.");
        }
    }
    public static void AgainstNullOrWhitespace(string? objectValue, string objectName)
    {
        if (string.IsNullOrWhiteSpace(objectValue))
        {
            throw new ArgumentException($"Value of '{objectName}' cannot be null or whitespace.");
        }
    }
    public static void AgainstEmptyConfiguration(IConfigurationProvider configurationProvider)
    {
        if (configurationProvider?.Configuration == null)
        {
            throw new InvalidOperationException("FFVM has not yet been configured.");
        }

        if (string.IsNullOrWhiteSpace(configurationProvider?.Configuration?.InstallationPath))
        {
            throw new InvalidOperationException("FFVM installation path has not been set.");
        }
    }
}
