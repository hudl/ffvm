#load "utility/configuration.cake"
#load "utility/repo-metadata.cake"

using System;
using System.IO;
using System.Linq;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using Amazon.IdentityManagement;
using Amazon.IdentityManagement.Model;
using Amazon.Runtime.CredentialManagement;
using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.SharedInterfaces;

Task("check")
    .IsDependentOn("check-environment")
    .IsDependentOn("check-dotnet")
    .IsDependentOn("check-docker")
    .IsDependentOn("check-install-script");

Task("check-environment")
    .Does(() =>
    {
        if (!System.IO.Directory.Exists(RepoMetadata.Current.InstallPath))
        {
            System.IO.Directory.CreateDirectory(RepoMetadata.Current.InstallPath);
        }

        if (!System.IO.Directory.Exists(RepoMetadata.Current.BuildPath))
        {
            System.IO.Directory.CreateDirectory(RepoMetadata.Current.BuildPath);
        }

        var pathEnvVariable = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Path" : "PATH";
        var pathEnv = System.Environment.GetEnvironmentVariable(pathEnvVariable);

        var pathSeparatorChar = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ";" : ":";
        var pathDirectories = pathEnv.Split(pathSeparatorChar, StringSplitOptions.RemoveEmptyEntries);
        var pathExists = pathDirectories.Any(p => p?.Equals(RepoMetadata.Current.InstallPath, StringComparison.OrdinalIgnoreCase) ?? false);
        if (!pathExists)
        {
            throw new Exception($"FFVM install directory is not in path: " + RepoMetadata.Current.InstallPath);
        }

        Information($"FFVM Environment:");
        Information($" RID:             {RepoMetadata.Current.RuntimeId}");
        Information($" Install Path:    {RepoMetadata.Current.InstallPath}");
        Information($" Build Path:      {RepoMetadata.Current.BuildPath}");
    })
    .OnError((e) =>
    {
        Error("Failed to find FFVM installation path in your env:PATH variable.");
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Error($"Run these commands in your terminal to add FFVM to your PATH:");
            Error($"  echo >> {RepoMetadata.Current.HomePath}/.profile");
            Error($"  echo 'export PATH=\"{RepoMetadata.Current.InstallPath}:$PATH\"' >> {RepoMetadata.Current.HomePath}/.profile");
            Error($"  source {RepoMetadata.Current.HomePath}/.profile");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Error($"Run these commands in your terminal to add FFVM to your PATH:");
            Error($"  echo >> {RepoMetadata.Current.HomePath}/.zprofile");
            Error($"  echo 'export PATH=\"{RepoMetadata.Current.InstallPath}:$PATH\"' >> {RepoMetadata.Current.HomePath}/.zprofile");
            Error($"  source {RepoMetadata.Current.HomePath}/.zprofile");
        }
        else
        {
            Error($"Follow these instructions to add FFVM to your PATH:");
            Error($"  1. Right-click on 'This PC' or 'Computer' in the file explorer.");
            Error($"  2. Click 'Properties'.");
            Error($"  3. Click 'Advanced system settings'.");
            Error($"  4. Click 'Environment Variables'.");
            Error($"  5. In the 'System variables' section, select 'Path' and click 'Edit'.");
            Error($"  6. Click 'New' and add the path to the FFVM install directory: {RepoMetadata.Current.InstallPath}");
            Error($"  7. Click 'OK' on all the dialogs to save your changes.");
        }
    });

Task("check-dotnet")
    .Does(() =>
    {
        var dotnetVersion = ProcessHelpers.RunProcessWithStdOut("dotnet", "--version");
        Information($"Dotnet Version: {dotnetVersion}");
        if (!Version.TryParse(dotnetVersion, out var version))
        {
            throw new Exception("FFVM requires dotnet, no installation was detected.");
        }

        if (version.Major < 8)
        {
            throw new Exception($"FFVM requires dotnet-sdk 8 or higher, found version: {dotnetVersion}.");
        }
    })
    .OnError((e) =>
    {
        Error("Failed to validate Microsoft .NET installation.");
        Error(e.Message);
    });

Task("check-docker")
    .Does(() =>
    {
        var dockerVersion = ProcessHelpers.RunProcessWithStdOut("docker", "--version");
        if (!dockerVersion.Contains("Docker version"))
        {
            throw new Exception("FFVM requires Docker, no installation was detected.");
        }

        Information($"Docker CLI Version:      {dockerVersion}");
    })
    .OnError((e) =>
    {
        Error("Failed to validate Microsoft .NET installation.");
        Error(e.Message);
    });

Task("check-install-script")
    .WithCriteria(() => RepoMetadata.Current.ConfigurationScript != null)
    .Does(() =>
    {
        Information($"Custom installation script file has been loaded.");
        foreach (var customVariable in RepoMetadata.Current.ConfigurationScript.Variables) 
        { 
            var variableLoadedLabel = $"Variable loaded '{customVariable.Name}':".PadRight(38); 
            Information($"{variableLoadedLabel}{customVariable.ActualValue}");
        }
    })
    .OnError((e) =>
    {
        Error(e.Message.Replace("\n", Environment.NewLine)); 
    });