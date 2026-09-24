using FFVM.Base.Extensions;
using Xunit;

namespace FFVM.Base.Tests.Extensions;

public class StringUrlExtensionsTests
{
    [Theory]
    [InlineData("https://example.com/video.mp4", true)]
    [InlineData("http://example.com/video.mp4", true)]
    [InlineData("rtmp://streaming.example.com/live/stream", true)]
    [InlineData("rtmps://streaming.example.com/live/stream", true)]
    [InlineData("s3://bucket/path/to/file.mp4", true)]
    [InlineData("ftp://files.example.com/file.mp4", true)]
    [InlineData("udp://239.0.0.1:1234", true)]
    [InlineData("https\\://example.com/video.mp4", true)] // backslash-escaped form used in filtergraph option values
    [InlineData("https\\://main.app.thorhudl.com/fx-renderer/ad2drender?accessKey=", true)] // real ad2drender filtergraph URL
    [InlineData("/local/path/to/file.mp4", false)]
    [InlineData("C://path/to/file.mp4", false)] // Windows drive letter — single char before ://
    [InlineData("D://path/to/file.mp4", false)]
    [InlineData("relative/path/file.mp4", false)]
    [InlineData("[0:v]scale=1280:720[out]", false)]
    [InlineData("scale=iw/2:ih/2", false)]
    [InlineData("output.mp4", false)]
    [InlineData("", false)]
    [InlineData("movie=/tmp/foo.png,overlay=https://example.com/x.png", false)] // filtergraph containing a URL is not itself a URL
    [InlineData("/tmp/https://embedded/in/path.mp4", false)] // URL-looking substring inside a local path
    [InlineData("subtitles=/tmp/file.srt:force_style='FontName=Arial'", false)]
    public void IsUrl_DetectsUrlsCorrectly(string value, bool expected)
    {
        Assert.Equal(expected, value.IsUrl());
    }
}
