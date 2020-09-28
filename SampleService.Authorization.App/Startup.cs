using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

using SampleService.Authorization.App.Services;
using SampleService.Authorization.Data;

using SampleService.Graphql;
using SampleService.Repositories;

namespace SampleService.Authorization.App
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddLogging();

            //services.AddDbContext<DataContext>(x => x.UseInMemoryDatabase("SampleDatabase"));
            var defaultConnection = Configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<DataContext>(x =>
            {
                x.UseSqlServer(defaultConnection, options => {
                    options.MigrationsAssembly("SampleService.Data.SqlServer");
                });
            }, ServiceLifetime.Singleton);

            services.TryAddSingleton<DataContext>();
          

            services.AddCors();

            services.AddControllers().AddJsonOptions(x => x.JsonSerializerOptions.IgnoreNullValues = true);

            var appSettingsSection = Configuration.GetSection("AppSettings");
            var environmentVariableSecret = Environment.GetEnvironmentVariable("ASPNETCORE_SECRET");
            if (String.IsNullOrWhiteSpace(environmentVariableSecret))
            {
                services.Configure<AppSettings>(appSettingsSection);
            }
            else
            {
                services.Configure<AppSettings>(options =>
                {
                    options.Secret = environmentVariableSecret;
                });
            }

            var appSettings = appSettingsSection.Get<AppSettings>();
            var key = Encoding.UTF8.GetBytes(appSettings.Secret);
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    // 토큰 만료 시간에 토큰이 정확하게 만료되도록 clockskew를 0으로 설정
                    ClockSkew = TimeSpan.Zero,
                };
            });

            services.TryAddTransient<IUserService, UserService>();
            services.TryAddTransient<IHasher, Hasher>();
            services.TryAddTransient<IUserRepository, UserRepository>();
            
            services.AddGraphql();

            services.Configure<KestrelServerOptions>(options => {
                options.AllowSynchronousIO = true;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHttpsRedirection();
            }

            app.UseRouting();

            app.UseCors(x => x
            .SetIsOriginAllowed(origin => true)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()
            );

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseGraphql();
        }
    }
}
