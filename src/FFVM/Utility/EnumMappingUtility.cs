using System.Reflection;

namespace FFVM.Base.Utility;

public class EnumMappingUtility
{
    public static List<EnumMapping<TEnumType>> GetEnumMappings<TEnumType>()
        where TEnumType : struct, Enum
    {
        var enumType = typeof(TEnumType);
        return Enum.GetNames(enumType)
            .Select(enumName =>
            {
                if (!Enum.TryParse<TEnumType>(enumName, true, out TEnumType enumValue))
                {
                    throw new Exception($"Could not parse enum type {enumName}");
                }

                var enumMember = enumType.GetMember(enumName)?.FirstOrDefault();
                var enumMappingAttribute = enumMember?.GetCustomAttribute<MappingAttribute>();

                return new EnumMapping<TEnumType>(enumValue, enumMappingAttribute);
            })
            .Where(mapping => mapping.MappingAttribute != null)
            .ToList();
    }
}
