/// <summary>
/// Static class for assisting with special treatment of raw arguments.
/// </summary>
static class Arguments
{
    private static readonly Lazy<List<ParsedArgument>> AllParsedArgs = new Lazy<List<ParsedArgument>>(() => 
    {
        // This just has to match the build scripts and start with a `-` but not be `-` or `--`
        var argPrefix = "--:";
        return Environment.GetCommandLineArgs()
            // Skip the Cake process and optional script name arguments
            .SkipWhile(arg => !arg.StartsWith('-'))
            .Select(arg => {
                return new ParsedArgument
                {
                    Value = arg.StartsWith(argPrefix) ? arg.Substring(argPrefix.Length) : arg,
                };
            })
            .ToList();
    });

    public static List<ParsedArgument> GetParsedArgs()
    {
        return AllParsedArgs.Value;
    }

    public class ParsedArgument
    {
        public string Value { get; set; }
        public bool IsPrefixed => Value.StartsWith('-');
    }
}
