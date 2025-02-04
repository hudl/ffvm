namespace FFVM.Base.Utility.BaseTypes;

public interface ICommandParameterFormatter
{
    object? FormatValue(string rawValue);
}
