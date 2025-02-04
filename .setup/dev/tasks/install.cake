#load "utility/io-helpers.cake"
#load "utility/process-helpers.cake"
#load "utility/publish-helpers.cake"
#load "utility/repo-metadata.cake"

/*
1. check for ffvm PATH location in machine path
  - EXIT if path is not set with instructions on how to add it for each system. 
  - WINDOWS: %USER_PROFILE%/bin
  - LINUX/OSX: $HOME/bin

2. install to PATH Location
  WINDOWS:  
    1. renaming files in location will cause re-resolution, existing emulation will work. 
  LINUX/OSX
    1. create symlinks to native.ffmpeg, if present
    2. create symlinks to emulate.ffmpeg
    3. move around ffmpeg files as necessary

*/
Task("install")
    .IsDependentOn("check")
    .IsDependentOn("clean-installation")
    .IsDependentOn("build-applications")
    .IsDependentOn("copy-applications")
    .IsDependentOn("configure-installation")
    .ReportError(exception =>
    {
        Error("Can't setup local environment for ffvm usage.");
        throw exception;
    });


Task("clean-installation")
    .Does(() =>
    {
        var ffvmExecutable = IOHelpers.GetFFVMExecutableName();
        var ffmpegExecutable = IOHelpers.GetFFmpegExecutableName();
        var ffprobeExecutable = IOHelpers.GetFFprobeExecutableName();
        var homePath = RepoMetadata.Current.HomePath;
        var ffvmInstallPath = System.IO.Path.Combine(RepoMetadata.Current.InstallPath, ffvmExecutable);
        var ffmpegInstallPath = System.IO.Path.Combine(RepoMetadata.Current.InstallPath, ffmpegExecutable);
        var ffprobeInstallPath = System.IO.Path.Combine(RepoMetadata.Current.InstallPath, ffprobeExecutable);

        IOHelpers.CleanDirectory(RepoMetadata.Current.BuildPath);
        IOHelpers.DeleteIfExists(ffvmInstallPath, ".ffvm");
        IOHelpers.DeleteIfExists(ffmpegInstallPath, ".ffvm", ".native");
        IOHelpers.DeleteIfExists(ffprobeInstallPath, ".ffvm", ".native");
    });

Task("full-clean-installation")
    .IsDependentOn("clean-installation")
    .Does(() =>
    {
        IOHelpers.DeleteIfExists(RepoMetadata.Current.ConfigurationPath);
    });

Task("build-applications")
    .Does(() =>
    {
        //publish source for local environment, using dotnet publish with single file output. 
        Information($"Publishing FFMpeg Emulator application...");
        PublishHelpers.PublishDotNet(PublishHelpers.FFVM_FFMPEG_APPLICATION);
        Information($"Publishing FFProbe Emulator application...");
        PublishHelpers.PublishDotNet(PublishHelpers.FFVM_FFPROBE_APPLICATION);
        Information($"Publishing FFVM application...");
        PublishHelpers.PublishDotNet(PublishHelpers.FFVM_MANAGER_APPLICATION);
    })
    .OnError((e) =>
    {
        Error("Failed to publish FFVM applications, check publish logs and try again.");
    });


Task("copy-applications")
    .Does(() =>
    {
        var ffvmExecutable = IOHelpers.GetFFVMExecutableName();
        var ffmpegExecutable = IOHelpers.GetFFmpegExecutableName();
        var ffprobeExecutable = IOHelpers.GetFFprobeExecutableName();
        var ffvmBuildPath = System.IO.Path.Combine(RepoMetadata.Current.BuildPath, ffvmExecutable);
        var ffvmInstallPath = System.IO.Path.Combine(RepoMetadata.Current.InstallPath, ffvmExecutable);
        var ffmpegBuildPath = System.IO.Path.Combine(RepoMetadata.Current.BuildPath, ffmpegExecutable);
        var ffmpegInstallPath = System.IO.Path.Combine(RepoMetadata.Current.InstallPath, ffmpegExecutable);
        var ffprobeBuildPath = System.IO.Path.Combine(RepoMetadata.Current.BuildPath, ffprobeExecutable);
        var ffprobeInstallPath = System.IO.Path.Combine(RepoMetadata.Current.InstallPath, ffprobeExecutable);
        if (IsRunningOnWindows())
        {
            System.IO.File.Copy(ffvmBuildPath, ffvmInstallPath, true);
            System.IO.File.Copy(ffmpegBuildPath, ffmpegInstallPath, true);
            System.IO.File.Copy(ffprobeBuildPath, ffprobeInstallPath, true);
        }
        else
        {
            //find existing ffmpeg installation location 
            var ffmpegNativeInstallationLocation = ProcessHelpers.RunProcessWithStdOut("which", "ffmpeg");
            var ffprobeNativeInstallationLocation = ProcessHelpers.RunProcessWithStdOut("which", "ffprobe");

            //creates symlinks for the ffvm emulator and proxy applications
            ProcessHelpers.RunProcess("ln", $"-s \"{ffvmBuildPath}\" \"{ffvmInstallPath}\"");
            ProcessHelpers.RunProcess("ln", $"-s \"{ffmpegBuildPath}\" \"{ffmpegInstallPath}\"");
            ProcessHelpers.RunProcess("ln", $"-s \"{ffmpegBuildPath}\" \"{ffmpegInstallPath}.ffvm\"");
            ProcessHelpers.RunProcess("ln", $"-s \"{ffprobeBuildPath}\" \"{ffprobeInstallPath}\"");
            ProcessHelpers.RunProcess("ln", $"-s \"{ffprobeBuildPath}\" \"{ffprobeInstallPath}.ffvm\"");

            //creates the symlinks for native installations, to simplify the installation workflow
            if (!string.IsNullOrWhiteSpace(ffmpegNativeInstallationLocation))
            {
                ffmpegNativeInstallationLocation = ffmpegNativeInstallationLocation.Trim();
                Information($"FFmpeg native installation located at: {ffmpegNativeInstallationLocation}");
                ProcessHelpers.RunProcess("ln", $"-s \"{ffmpegNativeInstallationLocation}\" \"{ffmpegInstallPath}.native\"");

            }
            if (!string.IsNullOrWhiteSpace(ffprobeNativeInstallationLocation))
            {
                ffprobeNativeInstallationLocation = ffprobeNativeInstallationLocation.Trim();
                Information($"FFProbe native installation located at: {ffprobeNativeInstallationLocation}");
                ProcessHelpers.RunProcess("ln", $"-s \"{ffprobeNativeInstallationLocation}\" \"{ffprobeInstallPath}.native\"");
            }
        }
    })
    .OnError((e) =>
    {
        Error("Failed to publish FFVM applications, check publish logs and try again.");
    });


Task("configure-installation")
    .Does(() =>
    {
        var ffvmConfigurationAlreadyExists = System.IO.File.Exists(RepoMetadata.Current.ConfigurationPath);

        //set the ffvm installation config settings
        Information($"Configuring FFVM Installation...");
        ProcessHelpers.RunWithFailure("ffvm", $"set installation-path  \"{RepoMetadata.Current.InstallPath}\"", "Failed to set FFVM installation path.");

        if (!string.IsNullOrWhiteSpace(RepoMetadata.Current.ConfigurationScript?.SharedSSOProfileName))
        { 
            ProcessHelpers.RunWithFailure("ffvm", $"set sso-profile \"{RepoMetadata.Current.ConfigurationScript?.SharedSSOProfileName}\"", "Failed to set FFVM SSO profile.");
        }

        if (ffvmConfigurationAlreadyExists)
        {
            Information($"FFVM configuration file already exists at: {RepoMetadata.Current.ConfigurationPath}");
            Information($"Skipping repository, and image setup. For a fresh install, first run the uninstall command.");
            return;
        }

        foreach (var repository in RepoMetadata.Current.ConfigurationScript?.Repositories) 
        { 
            Information($"Adding {repository.Name} image repository...");
            var defaultText = repository.IsDefault ? "--default" : string.Empty;
            ProcessHelpers.RunWithFailure("ffvm", $"add-repository {repository.Url} {repository.Name} --profile {repository.Auth} {defaultText}", "Failed to add repository.");
        }

        foreach (var image in RepoMetadata.Current.ConfigurationScript?.Images) 
        {
            Information($"Downloading image '{image}'..."); 
            ProcessHelpers.RunWithFailure("ffvm", $"install {image}", "Failed to install image.");
        }

        if (!string.IsNullOrWhiteSpace(RepoMetadata.Current.ConfigurationScript?.DefaultImage))
        { 
            Information($"Setting default image '{RepoMetadata.Current.ConfigurationScript?.DefaultImage}'...");
            ProcessHelpers.RunWithFailure("ffvm", $"use {RepoMetadata.Current.ConfigurationScript?.DefaultImage}", "Failed to set Production image as default.");

        }

        Information($"Successfully configured FFVM installation.");
    })
    .OnError((e) =>
    {
        Error("Failed to configure FFVM applications, check logs and try again.");
    });