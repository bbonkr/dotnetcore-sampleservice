using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

using SampleService.Data;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

namespace SampleService.Database.Manager
{
    class Program
    {
        private const string PREFIX = "APP_";
        private const string APP_SETTINGS_FILENAME = "appsettings";
        private const string HOST_SETTINGS_FILENAME = "hostsettings";
        private const string SETTINGS_FILE_EXT = ".json";

        static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) => new HostBuilder()
                .ConfigureHostConfiguration(config =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory());
                    config.AddJsonFile($"{HOST_SETTINGS_FILENAME}{SETTINGS_FILE_EXT}", optional: true);
                    config.AddEnvironmentVariables(prefix: PREFIX);
                    config.AddCommandLine(args);
                })
                .ConfigureAppConfiguration((context, config) =>
                {
                    // Application Configuration
                    config.SetBasePath(Directory.GetCurrentDirectory());
                    config.AddJsonFile($"{APP_SETTINGS_FILENAME}{SETTINGS_FILE_EXT}", optional: true);
                    config.AddJsonFile($"{APP_SETTINGS_FILENAME}.{context.HostingEnvironment.EnvironmentName}{SETTINGS_FILE_EXT}", optional: true);
                    config.AddEnvironmentVariables(prefix: PREFIX);
                    config.AddCommandLine(args);
                })
                //.ConfigureContainer(config => {
                //    // Configure the instantiated dependency injection container

                //})
                .ConfigureServices((context, services) =>
                {
                    //Adds services to the Dependency Injection container

                    services.AddLogging();
                    services.Configure<AppSettings>(context.Configuration);

                    var defaultConnection = context.Configuration.GetConnectionString("DefaultConnection");

                    services.AddDbContext<DataContext>(x =>
                    {
                        x.UseSqlServer(defaultConnection, options => {
                            options.MigrationsAssembly("SampleService.Data.SqlServer");
                        });
                    });

                    services.AddHostedService<App>();
                })
                .ConfigureLogging((context, logging) =>
                {
                    //Configure Logging
                    logging.AddConsole();
                })
                .UseConsoleLifetime(options => {});

    }
}
