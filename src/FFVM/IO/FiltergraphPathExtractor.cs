using System.Text.RegularExpressions;
using FFVM.Base.Extensions;

namespace FFVM.Base.IO;

// Extracts local file paths embedded in an ffmpeg filtergraph expression
// (e.g. -filter_complex "movie=/tmp/overlay.png[ol];..."). Callers use the
// returned paths to register docker volume mounts; the emitted filtergraph
// string itself is rewritten later by the parser's prefix-replace pass.
public static class FiltergraphPathExtractor
{
    // A URL scheme, optionally with a filtergraph-escaped colon (e.g. https\://).
    // The tail accepts escaped colons (\:) and any char that isn't a filter-node
    // terminator — so a URL inside a single filter node stops at the next
    // UNescaped ':' (the filter arg separator), not at the end of the whole
    // filter chunk. Without this, a following 'data=/path/...' inside the same
    // node would be masked and never emitted as a path.
    private static readonly Regex _urlPattern = new(
        @"[A-Za-z][A-Za-z0-9+.\-]+\\?:(?://|\\/\\/)(?:\\:|[^\s,;\[\]'"":])*",
        RegexOptions.Compiled);

    // A rooted local path — Unix (/foo/bar) or Windows drive-letter (C:/foo, C:\foo, C\:/foo).
    // Anchored at the start of the string or after a filtergraph token delimiter so we
    // don't grab option keys or numeric tuples that happen to contain '/'. Stops at ':'
    // because ':' separates filter-arg values (e.g. data=/foo.json:width=608).
    private static readonly Regex _pathPattern = new(
        @"(?:^|(?<=[=,;\[\]'""]))(?<path>/[^\s,;\[\]'""=:]+|[A-Za-z]\\?:[\\/][^\s,;\[\]'""=:]+)",
        RegexOptions.Compiled);

    public static IEnumerable<string> ExtractPaths(string filtergraph)
    {
        if (string.IsNullOrWhiteSpace(filtergraph))
        {
            yield break;
        }

        // Mask URLs with equal-length spaces so path offsets remain stable and no URL
        // substring can be matched as a path.
        var masked = _urlPattern.Replace(filtergraph, match => new string(' ', match.Length));

        foreach (Match match in _pathPattern.Matches(masked))
        {
            var candidate = match.Groups["path"].Value;
            if (string.IsNullOrWhiteSpace(candidate) || candidate.IsUrl())
            {
                continue;
            }
            yield return candidate;
        }
    }
}
