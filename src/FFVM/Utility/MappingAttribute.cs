namespace FFVM.Base.Utility;

[AttributeUsage(AttributeTargets.Field)]
public class MappingAttribute(string value) : Attribute
{
    public string Value { get; set; } = value;
}
