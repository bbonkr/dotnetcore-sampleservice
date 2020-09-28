using System;
using System.Reflection;

using GraphQL;
using GraphQL.Server;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore;
using Microsoft.Extensions.DependencyInjection;

using SampleService.Services;
using SampleService.Services.Schema;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SampleService.Services.GraphqlQueries;
using SampleService.Repositories;

namespace SampleService.Graphql
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddGraphql(this IServiceCollection services)
        {
            services.AddSingleton<IDependencyResolver>(x => new FuncDependencyResolver(type =>
            {
                //using (var scope = x.CreateScope())
                //{
                //    return scope.ServiceProvider.GetRequiredService(type);
                //}
                return x.GetRequiredService(type);
            }));


            services.TryAddTransient<IUserRepository, UserRepository>();

            services.AddSingleton<UserQuery>();
            services.AddSingleton<UserType>();
            services.AddSingleton<UserSchema>();

            var assembly = Assembly.GetAssembly(typeof(SampleService.Services.Schema.UserSchema));

            services
                .AddGraphQL()
                .AddWebSockets()
                .AddGraphTypes(assembly, ServiceLifetime.Singleton);

            return services;
        }

        public static IApplicationBuilder UseGraphql(this IApplicationBuilder app)
        {
            var graphqlOptions = new GraphQL.Server.Ui.GraphiQL.GraphiQLOptions {
                GraphiQLPath = "/ui/graphql",
                GraphQLEndPoint = "/graphql"
            };

            app.UseWebSockets();
            app.UseGraphQLWebSockets<UserSchema>();
            app.UseGraphQL<UserSchema>();
            app.UseGraphiQLServer(graphqlOptions);

            return app;
        }
    }
}
