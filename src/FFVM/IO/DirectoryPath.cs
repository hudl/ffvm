using FFVM.Base.Extensions;

namespace FFVM.Base.IO;

public class DirectoryPath
{
    private const string MappedDirectoryPrefix = "/mnt";
    private readonly string _originalFileName = string.Empty;
    private readonly string _originalDirectory = string.Empty;
    private readonly string _mappedDirectory = string.Empty;
    private readonly int _directoryParentCount = 0;
    private readonly int _directoryId = 0;

    public DirectoryPath()
    {
        _directoryId = int.MaxValue;
        _originalDirectory = Directory.GetCurrentDirectory().SanitizeUrl();
        _mappedDirectory = Path.Combine(MappedDirectoryPrefix, _directoryId.ToString()).SanitizeUrl();
        _directoryParentCount = _originalDirectory.Split("/").Length;
    }
    private DirectoryPath(string validPathUri, int pathIndex)
    {
        _directoryId = pathIndex;
        _originalFileName = Path.GetFileName(validPathUri);
        _originalDirectory = Path.IsPathRooted(validPathUri) ? Path.GetDirectoryName(validPathUri)!.SanitizeUrl() : Path.GetDirectoryName(Path.GetFullPath(validPathUri))!.SanitizeUrl();
        _mappedDirectory = Path.Combine(MappedDirectoryPrefix, _directoryId.ToString()).SanitizeUrl();
    }

    public int DirectoryId { get => _directoryId; }
    public int DirectoryCount { get => _directoryParentCount; }
    public string OriginalUri { get => Path.Combine(_originalDirectory, _originalFileName).SanitizeUrl(); }
    public string OriginalDirectory { get => _originalDirectory; }
    public string MappedUri { get => Path.Combine(_mappedDirectory, _originalFileName).SanitizeUrl(); }
    public string MappedDirectory { get => _mappedDirectory; }

    public static bool TryParse(string[] args, int index, out DirectoryPath? pathValidityCheckerMapper)
    {
        pathValidityCheckerMapper = default;
        try
        {
            var currArgument = args.Length > index ? args[index] : string.Empty;
            var prevArgument = args.Length > index - 1 && index > 0 ? args[index - 1] : string.Empty;
            if (string.IsNullOrWhiteSpace(currArgument) ||
                string.IsNullOrWhiteSpace(prevArgument))
            {
                return false;
            }

            if (prevArgument.StartsWith("-i", StringComparison.OrdinalIgnoreCase))
            {
                pathValidityCheckerMapper = new DirectoryPath(currArgument, index);
                return true;
            }

            var pathExtension = Path.GetExtension(currArgument);
            var pathFileName = Path.GetFileName(currArgument);
            var pathDirectory = Path.GetDirectoryName(currArgument);
            if (!string.IsNullOrWhiteSpace(pathExtension) &&
                !string.IsNullOrWhiteSpace(pathFileName) &&
                !string.IsNullOrWhiteSpace(pathDirectory))
            {
                pathValidityCheckerMapper = new DirectoryPath(currArgument, index);
                return true;
            }

            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }

}