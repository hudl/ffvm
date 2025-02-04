using System.Text.RegularExpressions;

namespace FFVM.Manager.Utility.Extensions;

public static class RegexExtensions
{
    public static Match MatchExpression(this Regex regex, string inputString) => regex.Match(inputString);
    public static Match GetNamedGroupValue(this Match matchRegex, string groupName, Action<string> setCapturedValue)
    {
        if (!matchRegex.Success)
        {
            return matchRegex;
        }
        var capturedValue = matchRegex.Groups[groupName];
        if (capturedValue == null)
        {
            return matchRegex;
        }
        setCapturedValue(capturedValue.Value);
        return matchRegex;
    }
}
