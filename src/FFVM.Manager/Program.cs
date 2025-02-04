using FFVM.Base;
using FFVM.Base.Commands.BaseTypes;
using FFVM.Manager.Commands;
using FFVM.Manager.Commands.Image;
using FFVM.Manager.Commands.Repository;
using FFVM.Manager.Models;
using FFVM.Manager.Services;
using FFVM.Manager.Services.BaseTypes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FFVM.Manager
{
    internal class Program
    {
        public const string ProgramName = "ffvm";

        static async Task Main(string[] args)
        {
            var hostBuilder = CreateHostBuilder(args).Build();
            _ = hostBuilder.Services.GetRequiredService<ProgramContext>();

            await ProgramMain.Main(hostBuilder.Services, ProgramName, args);
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, builder) =>
                {
                    builder.SetBasePath(Directory.GetCurrentDirectory());
                })
                .ConfigureServices((context, services) =>
                {
                    //configure all base services and classes 
                    DependencyInjection.ConfigureServices(services);

                    services.AddSingleton<IDockerHubApiService, DockerHubApiService>();

                    //repository commands
                    services.AddTransient<ICommandHandler, AddRepositoryCommandHandler>();
                    services.AddTransient<ICommandHandler, RemoveRepositoryCommandHandler>();
                    services.AddTransient<ICommandHandler, ListRepositoryCommandHandler>();
                    services.AddTransient<ICommandHandler, SetRepositoryCommandHandler>();
                    //image commands
                    services.AddTransient<ICommandHandler, InstallImageCommandHandler>();
                    services.AddTransient<ICommandHandler, ListImageCommandHandler>();
                    services.AddTransient<ICommandHandler, RefreshImageCommandHandler>();
                    services.AddTransient<ICommandHandler, UninstallImageCommandHandler>();
                    services.AddTransient<ICommandHandler, UseImageCommandHandler>();
                    //general commands
                    services.AddTransient<ICommandHandler, EmulatorCommandHandler>();
                    services.AddTransient<ICommandHandler, SetCommandHandler>();

                    services.AddTransient<IContainerAuthenicationService<DockerHubAuthentication, DockerHubCredentials>, DockerHubAuthenticationService>();
                    services.AddTransient<IContainerAuthenicationService<AmazonEcrAuthentication, AmazonEcrCredentials>, AmazonEcrAuthenticationService>();
                    services.AddTransient<IFFUtilitiesSystemRepositoryService, AmazonEcrSystemRepositoryService>();
                    services.AddTransient<IFFUtilitiesSystemRepositoryService, DockerHubSystemRepositoryService>();
                    services.AddTransient<IFFUtilitiesSystemRepositoryServiceFactory, SystemRepositoryServiceFactory>();
                });
        }

    }
}