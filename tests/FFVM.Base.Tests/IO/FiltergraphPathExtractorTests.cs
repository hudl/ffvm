using System.Linq;
using FFVM.Base.IO;
using Xunit;

namespace FFVM.Base.Tests.IO;

public class FiltergraphPathExtractorTests
{
    [Fact]
    public void ExtractsSingleUnixPath()
    {
        var paths = FiltergraphPathExtractor.ExtractPaths("movie=/tmp/overlay.png[ol];[0][ol]overlay=10:10 [out]").ToList();
        Assert.Contains("/tmp/overlay.png", paths);
    }

    [Fact]
    public void IgnoresUrlsAndKeepsColocatedPath()
    {
        var filtergraph = "movie=/tmp/overlay.png[ol];[0][ol]overlay=10:10,ad2drender=url=https://example.com/renderer [out]";
        var paths = FiltergraphPathExtractor.ExtractPaths(filtergraph).ToList();
        Assert.Contains("/tmp/overlay.png", paths);
        Assert.DoesNotContain(paths, p => p.Contains("example.com"));
        Assert.DoesNotContain(paths, p => p.Contains("renderer"));
    }

    [Fact]
    public void IgnoresEscapedUrl()
    {
        var filtergraph = @"ad2drender=url=https\://example.com/renderer";
        var paths = FiltergraphPathExtractor.ExtractPaths(filtergraph).ToList();
        Assert.Empty(paths);
    }

    [Fact]
    public void ExtractsWindowsPathWithEscapedColon()
    {
        var paths = FiltergraphPathExtractor.ExtractPaths(@"movie=C\:/assets/overlay.png[ol]").ToList();
        Assert.Contains(@"C\:/assets/overlay.png", paths);
    }

    [Fact]
    public void ReturnsEmptyForFiltergraphWithoutPaths()
    {
        var paths = FiltergraphPathExtractor.ExtractPaths("[0:v]scale=1280:720[out]").ToList();
        Assert.Empty(paths);
    }

    [Fact]
    public void ExtractsMultipleDistinctPaths()
    {
        var filtergraph = "movie=/tmp/a.png[a];movie=/opt/assets/b.png[b];[a][b]hstack";
        var paths = FiltergraphPathExtractor.ExtractPaths(filtergraph).ToList();
        Assert.Contains("/tmp/a.png", paths);
        Assert.Contains("/opt/assets/b.png", paths);
    }

    [Fact]
    public void ReturnsEmptyForEmptyInput()
    {
        Assert.Empty(FiltergraphPathExtractor.ExtractPaths(""));
        Assert.Empty(FiltergraphPathExtractor.ExtractPaths("   "));
    }

    [Fact]
    public void PathStopsAtFilterArgColon()
    {
        var paths = FiltergraphPathExtractor.ExtractPaths(
            "advirtualcam=data=/var/folders/xxx/HASH/advirtualcam-data.json:width=608:height=1080").ToList();
        Assert.Contains("/var/folders/xxx/HASH/advirtualcam-data.json", paths);
    }

    [Fact]
    public void ExtractsPathFollowingEscapedUrlInSameFilterNode()
    {
        var filtergraph =
            @"ad2drender=url=https\://main.app.thorhudl.com/fx-renderer/ad2drender?accessKey=:data=/var/folders/xxx/HASH/data.json:cef_loglevel=4";
        var paths = FiltergraphPathExtractor.ExtractPaths(filtergraph).ToList();
        Assert.Contains("/var/folders/xxx/HASH/data.json", paths);
        Assert.DoesNotContain(paths, p => p.Contains("thorhudl"));
    }
}
