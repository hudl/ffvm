static class Targets
{
    private static readonly Dictionary<string, string> TargetAliases = new Dictionary<string, string>();

    private static string Target;

    public static void AddAlias(string alias, string taskName)
    {
        // Explcit Add() to throw on duplicates
        TargetAliases.Add(alias, taskName);
    }
    
    /// <remarks>
    /// Passing in ICakeContext because static classes don't have access to Cake aliases
    /// </remarks>
    public static string GetTargetToRun(ICakeContext context, string defaultTarget)
    {
        // General precedence: --target > -t > --help (when before implicit target) > implicit target > default target
        var args = Arguments.GetParsedArgs();
        // Default to the first plain argument as the target
        Target = args
            .Where(arg => !arg.IsPrefixed)
            .Select(arg => arg.Value)
            .FirstOrDefault();

        // --help before an implicit target argument takes precedence. After a target or `--`, it's considered
        // help for a subcommand
        if (args.Select(arg => arg.Value).TakeWhile(value => value != "--" && value != Target).Contains("--help"))
        {
            Target = "help";
        }

        // If target is explicitly set with option syntax, use it instead
        Target = context.Argument("target", context.Argument("t", Target ?? defaultTarget));
        
        // Use an alias mapping to convert short names to long
        if (TargetAliases.TryGetValue(Target, out string aliasedTarget))
        {
            Target = aliasedTarget;
        }

        return Target;
    }

    public static List<string> GetArgumentsForRunningTarget()
    {
        if (Target == null)
        {
            throw new InvalidOperationException($"{nameof(GetArgumentsForRunningTarget)} must be called after {nameof(GetTargetToRun)}, e.g., inside the task to be run.");
        }
        var targetName = Target;
        var aliases = TargetAliases
            .Where(kvp => kvp.Value == targetName)
            .Select(kvp => kvp.Key)
            .ToList();
        aliases.Add(targetName);
        
        // Generate list of possible implicit and explicit target arguments
        
        // Arguments that indicate the next argument will be the target
        var splitArgTargetFormats = new string[]
        {
            // PowerShell-style (are there more variations of this?)
            // TODO - support this properly
            // "-Target"
        };
        // Single arguments that contain the target and any target prefix
        var singleArgTargetFormats = aliases.SelectMany(alias => new[]
            {
                alias,
                $"-t={alias}",
                $"-t=\"{alias}\"",
                $"-target={alias}",
                $"-target=\"{alias}\"",
                $"--target={alias}",
                $"--target=\"{alias}\"",
            });
        var allTargetFormats = splitArgTargetFormats.Concat(singleArgTargetFormats).ToList();

        var argsStartingWithTarget = Arguments.GetParsedArgs()
            .SkipWhile(arg => !allTargetFormats.Contains(arg.Value, StringComparer.InvariantCultureIgnoreCase))
            .ToList();

        if (!argsStartingWithTarget.Any())
        {
            return new List<string>();
        }

        var hasSplitForm = splitArgTargetFormats.Contains(argsStartingWithTarget[0].Value, StringComparer.InvariantCultureIgnoreCase);
        if (hasSplitForm && !aliases.Contains(argsStartingWithTarget[1].Value))
        {
            throw new Exception($"Expecting target {argsStartingWithTarget[1]} to be one of {string.Join(", ", aliases)}.");
        }
        var argsAfterTarget = argsStartingWithTarget
            .Skip(hasSplitForm ? 2 : 1)
            .Where(arg => arg.Value != "--")
            .ToList();
    
        return argsAfterTarget.Select(arg => arg.Value).ToList();
    }
}

/// <remarks>
/// Feels a little odd, but this is a valid extension method because it gets wrapped in a static class by Cake. Wrapping
/// in a static class actually makes it nested and fails.
/// </remarks>
public static CakeTaskBuilder AddAlias(this CakeTaskBuilder builder, string alias)
{
    Targets.AddAlias(alias, builder.Task.Name);
    return builder;
}
