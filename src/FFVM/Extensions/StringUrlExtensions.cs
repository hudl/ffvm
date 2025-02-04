namespace FFVM.Base.Extensions;

public static class StringUrlExtensions
{
    public static string SanitizeUrl(this string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return url;
        }

        var urlReplaced = url.Replace("\\", "/");
        if (urlReplaced.IndexOf(':') > -1)
        {
            var drivePortion = urlReplaced[..urlReplaced.IndexOf(':')];
            urlReplaced = urlReplaced.Replace(drivePortion, drivePortion.ToLower());
        }

        //this handles fixing special escape sequences
        urlReplaced = urlReplaced.Replace("/:", "\\:");

        return urlReplaced;
    }
}
