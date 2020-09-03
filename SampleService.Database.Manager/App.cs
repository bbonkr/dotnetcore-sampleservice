using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using SampleService.Data;

namespace SampleService.Database.Manager
{
    public class App:IHostedService
    {
        public App(
            DataContext dataContext,
            IHostApplicationLifetime hostApplicationLifetime,
            IOptions<AppSettings> appSettings, 
            ILoggerFactory loggerFactory)
        {
            this.dataContext = dataContext;
            this.hostApplicationLifetime = hostApplicationLifetime;
            this.appSettings = appSettings.Value;
            logger = loggerFactory.CreateLogger<App>();            
        }


        public async Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation($"App run {appSettings.ConnectionStrings.DefaultConnection}");

            logger.LogInformation($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [START]\t\tDatabase migration ");
            await dataContext.Database.MigrateAsync(cancellationToken);
            logger.LogInformation($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [COMPLETE]\t\tDatabase migration");

            Console.WriteLine("데이터베이스 마이그레이션 완료");

            // 종료
            hostApplicationLifetime.StopApplication();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private readonly DataContext dataContext;
        private readonly IHostApplicationLifetime hostApplicationLifetime;
        private readonly AppSettings appSettings;
        private readonly ILogger logger;        
    }
}
