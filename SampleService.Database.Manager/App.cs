using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using SampleService.Data;

namespace SampleService.Database.Manager
{
    public class App:IHostedService
    {
        public App(DataContext dataContext, IOptions<AppSettings> appSettings, ILoggerFactory loggerFactory)
        {
            this.dataContext = dataContext;
            this.appSettings = appSettings.Value;
            logger = loggerFactory.CreateLogger<App>();
        }

 
        public Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation($"App run {appSettings.ConnectionStrings.DefaultConnection}");

            Console.ReadLine();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private readonly DataContext dataContext;
        private readonly AppSettings appSettings;
        private readonly ILogger logger;
    }
}
