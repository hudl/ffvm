using FFVM.Base.Utility.BaseTypes;

namespace FFVM.Base.Utility;

public class EnumParameterFormatter<TEnumType> : ICommandParameterFormatter
    where TEnumType : struct, Enum
{
    public object? FormatValue(string rawValue)
    {
        var enumMappings = EnumMappingUtility.GetEnumMappings<TEnumType>(); 
        var matchingValue = enumMappings.FirstOrDefault(map => map.MappingAttribute!.Value == rawValue);
        return matchingValue?.Value;
    }
}
