Task("Hudl Command Line Tools - Help")
    .AddAlias("help")
    .Does(() =>
{
    Information("Run FFVM:Setup commands as follows:");
    Information("");
    Information("> ./dev.sh COMMAND [args...]");
    Information("> ./dev.ps1 COMMAND [args...]");
    Information("");
    Information("Commands");
    Information("--------");
    Information("");
    Information("Running");
    Information("   check                        Checks the system for prerequisites needed to run ffvm.");
    Information("   install                      Installs ffvm in the local environment.");
    Information("   uninstall                    Uninstalls ffvm from the local environment, cleaning up symlinks.");
    Information("   help                         Shows this help text.");
});
