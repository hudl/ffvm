using FFVM.Base.Config.BaseTypes;
using FFVM.Base.IO;
using FFVM.Base.IO.BaseTypes;
using FFVM.Base.Shell.BaseTypes;
using FFVM.Base.Shell.Models;

namespace FFVM.Base.Shell;

public class FFUtilityEmulator(
    ILogger _logger, 
    ISystemShellRunner _systemShellRunner, 
    IConfigurationProvider _configurationProvider) : IFFUtilityEmulator
{
    public Task<int> Run(EmulatorRequest emulatorRequest)
    {
        if (_configurationProvider.Configuration == null)
        {
            _logger.WriteStdErr("Configuration was not found, try running ffvm to generate it.");
            return Task.FromResult(ExitCodes.GeneralError);
        }

        var imageVersion = _configurationProvider.Configuration.Versions.FirstOrDefault(v => v.IsSelected);
        if (imageVersion == null)
        {
            _logger.WriteStdErr($"There is no active version of {emulatorRequest.ProgramName}, run ffvm use to select one from your installed list.");
            return Task.FromResult(ExitCodes.GeneralError);
        }

        var sanitizedArgumentsAndPathResults = ArgumentParserAndSanitizer.GetSanitizedArgumentsAndPathResults(emulatorRequest.Arguments);

        var imageTag = $"{imageVersion.RegistryName}/{imageVersion.RepositoryName}:{imageVersion.ImageVersion}";

        //_logger.WriteStdErr(sanitizedArgumentsAndPathResults.FFUtilityArguments);

        var results = _systemShellRunner.RunCommand($"docker run --rm --entrypoint {emulatorRequest.ProgramName} {sanitizedArgumentsAndPathResults.DockerArguments} {imageTag}  {sanitizedArgumentsAndPathResults.FFUtilityArguments}", ShellCommandOptions.EchoStdErrOut());

        return Task.FromResult(results.ExitCode);
    }
}