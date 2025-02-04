using System.Reflection;

namespace FFVM.Base.Utility;

public class SetConfigurationParameterPair(PropertyInfo propertyInfo)
{
    public PropertyInfo PropertyInfo { get; set; } = propertyInfo;
    public SetConfigurationParameterAttribute? Attribute { get; set; } = propertyInfo.GetCustomAttribute<SetConfigurationParameterAttribute>();
}
