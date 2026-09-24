namespace FFVM.Base.Extensions;

public static class StringUrlExtensions
{
    public static bool IsUrl(this string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        var unescaped = value.Replace("\\:", ":");
        return Uri.TryCreate(unescaped, UriKind.Absolute, out var uri)
            && uri.Host.Length > 0;
    }

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
