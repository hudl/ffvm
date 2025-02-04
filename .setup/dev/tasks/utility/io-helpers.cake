using System.IO; 

static class IOHelpers
{ 
    public const string FFVM_MANAGER_EXECUTABLE = "ffvm";
    public const string FFVM_FFPROBE_EXECUTABLE = "ffprobe";
    public const string FFVM_FFMPEG_EXECUTABLE = "ffmpeg";

    private static string GetOSExcutableName(string applicationName) 
    { 
        var extension = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".exe" : "";
        return $"{applicationName}{extension}";
    } 

    public static string GetFFVMExecutableName() => GetOSExcutableName(FFVM_MANAGER_EXECUTABLE);
    public static string GetFFmpegExecutableName() => GetOSExcutableName(FFVM_FFMPEG_EXECUTABLE);
    public static string GetFFprobeExecutableName() => GetOSExcutableName(FFVM_FFPROBE_EXECUTABLE);
    
    public static void DeleteIfExists(string path, params string[] extensions)
    { 
        if (System.IO.File.Exists(path)) { 
            System.IO.File.Delete(path);
        }
        foreach(var extension in extensions) {
            if (System.IO.File.Exists($"{path}{extension}")) { 
                System.IO.File.Delete($"{path}{extension}");
            }
        }
    }

    public static void CleanDirectory(string directory)
    {
        var temporaryDirectory = new DirectoryInfo(directory);
        CleanDirectory(temporaryDirectory, false);
    }

    private static void CleanDirectory(DirectoryInfo directory, bool removeDir)
    {
        var subDirectories = directory.GetDirectories();
        if (subDirectories.Length > 0)
        {
            Array.ForEach(subDirectories, dir =>
            {
                CleanDirectory(dir, true);
            });
        }

        var temporaryFiles = directory.GetFiles();
        if (temporaryFiles.Length > 0)
        {
            Array.ForEach(temporaryFiles, file => System.IO.File.Delete(file.FullName));
        }

        if (removeDir)
        {
            directory.Delete(true);
        }
    }
}