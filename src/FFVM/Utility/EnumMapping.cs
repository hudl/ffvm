namespace FFVM.Base.Utility;

public class EnumMapping<TEnumType>(TEnumType value, MappingAttribute? mappingAttribute)
{
    public TEnumType Value { get; set; } = value;
    public MappingAttribute? MappingAttribute { get; set; } = mappingAttribute;
}
