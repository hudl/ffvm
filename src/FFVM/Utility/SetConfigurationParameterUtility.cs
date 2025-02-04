namespace FFVM.Base.Utility;

public class SetConfigurationParameterUtility
{
    public static Dictionary<string, SetConfigurationParameterPair> GetPropertiesAndAttributes<TObjectType>()
        where TObjectType : class => GetPropertiesAndAttributes(typeof(TObjectType));
    public static Dictionary<string, SetConfigurationParameterPair> GetPropertiesAndAttributes(Type? objectType)
    {
        if (objectType == null) return [];

        var rawObjectProperties = objectType?.GetProperties();
        return rawObjectProperties
            ?.Select(pi => new SetConfigurationParameterPair(pi))
            ?.Where(pi => pi.Attribute != null)
            ?.ToDictionary(pi => pi.Attribute!.Name, pi => pi) ?? [];
    }
}
