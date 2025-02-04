using FFVM.Base.Extensions;
using FFVM.Base.Utility;

namespace FFVM.Base.IO;

public class ArgumentParserAndSanitizer
{
    public static SanitizedArgumentsAndPathResults GetSanitizedArgumentsAndPathResults(string[] args)
    {
        var results = new SanitizedArgumentsAndPathResults();
        var listOfArguments = new List<string>();
        var listOfPaths = new List<DirectoryPath>
        {
            //add the current working directory for relative paths
            new()
        };

        //loop through to add directories from arguments
        for (var i = 0; i < args.Length; i++)
        {
            if (!DirectoryPath.TryParse(args, i, out DirectoryPath? directoryPath))
            {
                listOfArguments.Add(args[i]);
                continue;
            }

            listOfArguments.Add($"\"{args[i].SanitizeUrl()}\"");

            //iteratively look through parent directories to see if they exist
            AddToPathListIfParentDoesNotExist(listOfPaths, directoryPath!); 
        }

        results.FFUtilityArguments = string.Join(" ", listOfArguments);
        listOfPaths.ForEach(dp => { results.FFUtilityArguments = results.FFUtilityArguments.Replace($"{dp.OriginalDirectory}/", $"{dp.MappedDirectory}/"); });

        var workingDirectory = listOfPaths.FirstOrDefault(); 
        results.DockerArguments = $"-w {workingDirectory?.MappedDirectory} -v \"{string.Join("\" -v \"", listOfPaths.Select(p => $"{p.OriginalDirectory}:{p.MappedDirectory}"))}\"";

        return results; 
    }

    private static void AddToPathListIfParentDoesNotExist(List<DirectoryPath> pathList, DirectoryPath pathToAdd)
    {
        Guard.AgainstNull(pathList, nameof(pathList));
        Guard.AgainstNull(pathToAdd, nameof(pathToAdd));

        var currentDirectory = pathToAdd?.OriginalDirectory;
        while (!string.IsNullOrWhiteSpace(currentDirectory))
        {
            //check if the directory already exists in our paths from arguments
            if (pathList.Any(path => path?.OriginalDirectory?.Equals(currentDirectory, StringComparison.OrdinalIgnoreCase) ?? false))
            {
                //early exit because we have determined that the parent folder is already mounted.
                return;
            }

            try
            {
                currentDirectory = Path.GetDirectoryName(currentDirectory);
                if (string.IsNullOrWhiteSpace(currentDirectory))
                {
                    //we have reached the top level of the directory tree and so this must be a unique path
                    break;
                }

                //sanitize the url, as it has likely become broken due to windows different Path.DirectorySeparatorChar
                currentDirectory = currentDirectory.SanitizeUrl();
            }
            catch (Exception)
            {
                //a failure in here means that we are at the root level for all windows OSs
            }
        }

        pathList.Add(pathToAdd!);
    }

}