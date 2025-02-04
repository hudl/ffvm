namespace FFVM.Base.Utility;

[AttributeUsage(AttributeTargets.Property)]
public class SetConfigurationParameterAttribute(string name) : Attribute
{
    public string Name { get; set; } = name;
}
