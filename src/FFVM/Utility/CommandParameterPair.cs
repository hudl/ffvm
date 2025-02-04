using System.Reflection;

namespace FFVM.Base.Utility;

public class CommandParameterPair(PropertyInfo propertyInfo)
{
    public PropertyInfo PropertyInfo { get; set; } = propertyInfo;
    public CommandParameterAttribute? Attribute { get; set; } = propertyInfo.GetCustomAttribute<CommandParameterAttribute>();
}
