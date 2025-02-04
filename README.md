# FFVM
[![](https://img.shields.io/badge/hudl-OSS-orange.svg)](http://hudl.github.io/)

FFVM is a version manager, and docker emulator for FFmpeg & FFprobe. This command line tool works on MacOSX (Intel), MacOSX (Apple Silicon), Windows, Windows WSL, and Linux systems. The system works by translating local FFmpeg and FFprobe commands and translating them to Docker run commands, and mapping all the necessary drives. It makes it easier to quickly switch between versions of FFmpeg.

# Installing and Updating
## Prerequisites
Checking for prerequisites with ffvm is easy. Simply run the develop scripts below based on the terminal or OS that you use. Simply follow any instructions for installing missing prerequisites there. FFVM does require the following programs, and they will be checked in installation and check steps:
- Docker CLI
- .NET 8 SDK

*commands*
```
./dev.sh check
```
```
./dev.ps1 check
```

## Installing 
To *install* or *update* ffvm you should simply run one of the scripts below based on the terminal or OS that you use. Running the install command to *update* your instance of ffvm will not overwrite your configuration. If you need a clean re-install, first run the uninstall command. 

*commands* 
```
./dev.sh install
```
```
./dev.ps1 install
```

## Uninstalling 
To *uninstall* ffvm you should run one of the scriopts below based on the terminal or OS that you use. This will remove ffvm and the configuration file. 

*commands* 
```
./dev.sh uninstall
```
```
./dev.ps1 uninstall
```

# Usage
FFVM works by pulling container images from different repositories. Any container will work as long as ffmpeg and ffprobe are available from PATH in the container. The program then routes requests from the commmand line to the container for processing. It will map all file paths to the container, to ensure a seemless transition from native ffmpeg/ffprobe commands. 

## Adding repositories, and setting defaults.
Setting up repositories is fast and easy with ffvm. We currently support DockerHub and Amazon ECR repository types. We can automatically detect the different repository type based on the URLs provided. Before *installing* an image, you must first add a repository. The following commands work to add a repository. 
```
ffvm add-repository <repositoryUrl> <name> [OPTIONS]
```
The command above will validate the repository URL provided is a valid repository by querying available tags in the provided repository. Then the program will add this to a list of your active repositories list where you can then install images. You may specify any of the following options to further configure the command. 
```
 --default     This sets the repository as the default repository from which to pull images. This is helpful if you have a primary repository you wish to work off of. 
 --profile     For AmazonECR repositories, this is the AWS_PROFILE configured on your machine that you wish to use for authentication to the repository. 
 --user        For DockerHub repositories, this is the username used for authentication with dockerhub. 
 --password    For DockerHub repositories, this is the password or access token for authentication with dockerhub. 
``` 
We do not store your username or password. We use it to create a 12hr authentication token with Dockerhub so that we can query tags in the repository to list available tags. 

Below you can see an example of adding the *jrottenberg/ffmpeg* Docker container repository, giving it the name *jrottenberg* in ffvm. If you do not specify a dockerhub username and password, you will be prompted for one when it is needed.
```
ffvm add-repository jrottenberg/ffmpeg jrottenberg
```
To set this repository as the default repository, include the `--default` flag while adding the repository. 
```
ffvm add-repository jrottenberg/ffmpeg jrottenberg --default
```
Or you can change your default repository anytime by running the `set-repository` command. 
```
ffvm set-repository <name>
```
```
ffvm set-repository jrottenberg
```

## Installing, Uninstalling, & using versions.
Installing, and uninstalling images is easy as well with ffvm. 
```
ffvm install <imageTag> [OPTIONS]
```
```
ffvm install <repositoryName>:<imageTag> [OPTIONS]
```
If you omit the *repositoryName* from your command, then ffvm will assume the tag belongs to your default repository. Otherwise you may specify the *repositoryName* to add images from any repository at any time. 

The command above will perform a login (if authentication is required, AmazonECR), then pull the above image. After the image has been downloaded the container is validated and the version of ffmpeg will be pulled. You may specify any of the following options to further configure the command. 
```
 --name        This sets up a short name or alias for the image, defaults to FFMPEGVERSION_REPOSITORYNAME. 
```
We will tag your image locally by ffmpeg version, and a name that is either specified through the install command OPTIONS, or we calculate. 

To use an image, or switch versions, you can simply run the *use* command. 
```
ffvm use <imageName>
```
```
ffvm use <ffmpegVersion>
```
You can specify any name or ffmpeg version that exists in your system. FFVM will search your installed images and set the matching version as the active one. 

To remove an image, you can run the *uninstall* command. 
```
ffvm uninstall <imageName>
```
```
ffvm uninstall <ffmpegVersion>
```
You can specify any name or ffmpeg version that exists in your system. FFVM will search your installed images and remove the matching version. 

## Listing installed & available versions.
You can list your installed versions, or also list available versions for any repository that you have configured. 
```
ffvm list
```
```
ffvm list available [OPTIONS]
```
The first command will list the installed images on your system. The second command will list the available image tags from your default repository. 

You may specify any of the following options to further configure the `list available` command.
```
 --repo        This specifies the repository from which to query available tags, Defaults to the default repository name. 
```