namespace FFVM.Base.Utility;

[AttributeUsage(AttributeTargets.Property)]
public class CommandParameterAttribute(
    string name, 
    int order = -1, 
    string description = "", 
    bool isvalueless = true, 
    bool isrequired = false, 
    Type? customFormatterType = null) : Attribute
{
    public int? Order { get; set; } = order;
    public string Name { get; set; } = name;
    public string Description { get; set; } = description;
    public bool IsValueless { get; set; } = isvalueless;
    public bool IsRequired { get; set; } = isrequired;
    public Type? CustomFormatterType { get; set; } = customFormatterType;
}
