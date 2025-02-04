#load "./repo-metadata.cake"
#load "./process-helpers.cake"

using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;

static class PublishHelpers
{
    public const string FFVM_FFMPEG_APPLICATION = "FFVM.FFmpeg";
    public const string FFVM_FFPROBE_APPLICATION = "FFVM.FFprobe";
    public const string FFVM_MANAGER_APPLICATION = "FFVM.Manager";

    public static void PublishDotNet(string applicationName)
    {
        var buildDirectory = System.IO.Path.Combine(RepoMetadata.Current.SourcePath, applicationName, $"{applicationName}.csproj"); //CGB we need to figure out local paths

        ProcessHelpers.RunProcess("dotnet", $"publish \"{buildDirectory}\" --configuration Release --output {RepoMetadata.Current.BuildPath} --force --runtime {RepoMetadata.Current.RuntimeId} --no-self-contained -p:PublishSingleFile=true");
    }
}