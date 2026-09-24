using FFVM.Base.IO;
using Xunit;

namespace FFVM.Base.Tests.IO;

public class ArgumentParserAndSanitizerTests
{
    [Fact]
    public void TranslatesLocalPathInput()
    {
        var args = new[] { "-i", "/tmp/input.mp4", "output.mp4" };

        var result = ArgumentParserAndSanitizer.GetSanitizedArgumentsAndPathResults(args);

        Assert.DoesNotContain("/tmp/input.mp4", result.FFUtilityArguments);
        Assert.Contains("input.mp4", result.FFUtilityArguments);
        Assert.Contains("/mnt/", result.FFUtilityArguments);
        Assert.Contains("/tmp:/mnt/", result.DockerArguments);
    }

    [Fact]
    public void TranslatesUrlInputWithoutVolumeMounting()
    {
        var args = new[] { "-i", "https://example.com/video.mp4", "output.mp4" };

        var result = ArgumentParserAndSanitizer.GetSanitizedArgumentsAndPathResults(args);

        Assert.Contains("https://example.com/video.mp4", result.FFUtilityArguments);
        Assert.DoesNotContain("example.com", result.DockerArguments);
    }

    [Fact]
    public void TranslatesLocalPathsInsideFiltergraphAndPreservesUrls()
    {
        // Both the input and the filtergraph's overlay file are in /tmp, so /tmp/ is registered
        // via the -i arg. The Replace step translates that prefix inside the filtergraph too,
        // while the https:// URL passes through unchanged.
        var filtergraph = "movie=/tmp/overlay.png[ol];[0][ol]overlay=10:10,ad2drender=url=https://example.com/renderer [out]";
        var args = new[] { "-i", "/tmp/input.mp4", "-filter_complex", filtergraph, "output.mp4" };

        var result = ArgumentParserAndSanitizer.GetSanitizedArgumentsAndPathResults(args);

        Assert.DoesNotContain("/tmp/input.mp4", result.FFUtilityArguments);
        Assert.Contains("input.mp4", result.FFUtilityArguments);

        Assert.DoesNotContain("/tmp/overlay.png", result.FFUtilityArguments);
        Assert.Contains("overlay.png", result.FFUtilityArguments);

        Assert.Contains("https://example.com/renderer", result.FFUtilityArguments);

        Assert.DoesNotContain("example.com", result.DockerArguments);
    }

    [Fact]
    public void MountsFiltergraphPathWhenParentIsNotReferencedElsewhere()
    {
        // /opt/assets is only referenced from inside the filtergraph. The extractor must
        // register it so the docker command mounts the directory and the emitted filtergraph
        // uses the /mnt/<id> path.
        var filtergraph = "movie=/opt/assets/overlay.png[ol];[0][ol]overlay=10:10 [out]";
        var args = new[] { "-i", "/tmp/input.mp4", "-filter_complex", filtergraph, "output.mp4" };

        var result = ArgumentParserAndSanitizer.GetSanitizedArgumentsAndPathResults(args);

        Assert.Contains("/tmp:/mnt/", result.DockerArguments);
        Assert.Contains("/opt/assets:/mnt/", result.DockerArguments);
        Assert.DoesNotContain("/opt/assets/overlay.png", result.FFUtilityArguments);
        Assert.Contains("overlay.png", result.FFUtilityArguments);
    }

    [Fact]
    public void MountsMultipleDistinctFiltergraphPathsToSeparateMountPoints()
    {
        // Two paths from the same filtergraph whose parent directories are unrelated.
        // Each must get a distinct /mnt/<id> so docker mounts them independently.
        var filtergraph = "movie=/opt/assets/logo.png[a];movie=/var/overlays/bug.png[b];[0][a][b]overlay";
        var args = new[] { "-i", "/tmp/input.mp4", "-filter_complex", filtergraph, "output.mp4" };

        var result = ArgumentParserAndSanitizer.GetSanitizedArgumentsAndPathResults(args);

        Assert.Contains("/opt/assets:/mnt/", result.DockerArguments);
        Assert.Contains("/var/overlays:/mnt/", result.DockerArguments);
        // The two source dirs must not map to the same /mnt point.
        var optMount = System.Text.RegularExpressions.Regex.Match(result.DockerArguments, @"/opt/assets:(/mnt/\d+)").Groups[1].Value;
        var varMount = System.Text.RegularExpressions.Regex.Match(result.DockerArguments, @"/var/overlays:(/mnt/\d+)").Groups[1].Value;
        Assert.NotEmpty(optMount);
        Assert.NotEmpty(varMount);
        Assert.NotEqual(optMount, varMount);
    }

    [Fact]
    public void DoesNotMisclassifyFiltergraphEndingInFileExtensionToken()
    {
        // Regression guard: before the filter-flag short-circuit in TryParse, a filtergraph
        // whose last '.' fell after the last '/' was classified as a single file path and
        // mangled. It must now be treated as a filtergraph and left intact.
        var filtergraph = "[0:v]scale=1280:720[out];something.mp4";
        var args = new[] { "-i", "/tmp/input.mp4", "-filter_complex", filtergraph, "output.mp4" };

        var result = ArgumentParserAndSanitizer.GetSanitizedArgumentsAndPathResults(args);

        Assert.Contains(filtergraph, result.FFUtilityArguments);
        Assert.DoesNotContain("something.mp4:/mnt", result.DockerArguments);
    }

    [Theory]
    [InlineData("-vf")]
    [InlineData("-af")]
    [InlineData("-lavfi")]
    [InlineData("-filter_script")]
    public void TranslatesEmbeddedPathsForAllFilterFlags(string filterFlag)
    {
        var filtergraph = "movie=/tmp/overlay.png[ol];[0][ol]overlay=10:10 [out]";
        var args = new[] { "-i", "/tmp/input.mp4", filterFlag, filtergraph, "output.mp4" };

        var result = ArgumentParserAndSanitizer.GetSanitizedArgumentsAndPathResults(args);

        Assert.DoesNotContain("/tmp/overlay.png", result.FFUtilityArguments);
        Assert.Contains("overlay.png", result.FFUtilityArguments);
        Assert.Contains("/tmp:/mnt/", result.DockerArguments);
    }

    [Fact]
    public void WrapsFiltergraphValueInQuotes()
    {
        // DirectoryPathParser quotes the filtergraph value in the emitted command.
        // A missing quote would silently produce a malformed docker invocation.
        var filtergraph = "[0:v]scale=1280:720[out]";
        var args = new[] { "-i", "/tmp/input.mp4", "-filter_complex", filtergraph, "output.mp4" };

        var result = ArgumentParserAndSanitizer.GetSanitizedArgumentsAndPathResults(args);

        Assert.Contains($"-filter_complex \"{filtergraph}\"", result.FFUtilityArguments);
    }

    [Fact]
    public void TranslatesEscapedUrlInputWithoutVolumeMounting()
    {
        // FFmpeg uses https\:// (escaped colon) in filtergraph contexts. IsUrl() handles this
        // form, but the TryParse guard must also wire it correctly for top-level -i args.
        var args = new[] { "-i", @"https\://example.com/video.mp4", "output.mp4" };

        var result = ArgumentParserAndSanitizer.GetSanitizedArgumentsAndPathResults(args);

        Assert.Contains(@"https\://example.com/video.mp4", result.FFUtilityArguments);
        Assert.DoesNotContain("example.com", result.DockerArguments);
    }

    [Fact]
    public void MountsWindowsPathEmbeddedInFiltergraph()
    {
        if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
                System.Runtime.InteropServices.OSPlatform.Windows))
        {
            return;
        }

        var filtergraph = @"movie=C\:/assets/overlay.png[ol];[0][ol]overlay=10:10 [out]";
        var args = new[] { "-i", @"C\:/input/input.mp4", "-filter_complex", filtergraph, "output.mp4" };

        var result = ArgumentParserAndSanitizer.GetSanitizedArgumentsAndPathResults(args);

        Assert.Contains(@"C\:/assets:/mnt/", result.DockerArguments);
        Assert.DoesNotContain(@"C\:/assets/overlay.png", result.FFUtilityArguments);
    }

}
