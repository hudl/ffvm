Task("uninstall")
    .IsDependentOn("full-clean-installation")
    .ReportError(exception =>
    {
        Error("Failed to clean the current installation.");
        throw exception;
    })
    .Does(() => 
    {
        Information("FFVM uninstallation was successful, you may need to reload your session to update PATH.");
    });
