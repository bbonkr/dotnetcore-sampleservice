using System;
using System.Collections.Generic;
using System.Text;

using GraphQL.Types;

using SampleService.Repositories;
using SampleService.Services.Schema;

namespace SampleService.Services.GraphqlQueries
{
    public class UserQuery : ObjectGraphType<object>
    {
        public UserQuery(IUserRepository userRepository)
        {
            this.userRepository = userRepository;

            FieldAsync<ListGraphType<UserType>>("users",
                description: "Get list of user",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<IntGraphType>>()
                    {
                        Name = "page",
                        Description = "current page",
                        DefaultValue = 1,
                    }, new QueryArgument<NonNullGraphType<IntGraphType>>()
                    {
                        Name = "count",
                        Description = "Records per a page",
                        DefaultValue = 10,
                    }),
                resolve: async x => await userRepository.GetUsersAsync(
                    _ => true,
                    x.GetArgument<int>("page"),
                    x.GetArgument<int>("count"),
                    false
                    ));

            FieldAsync<UserType>(
                "findUserById",
                description: "Find user by id",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>>()
                    {
                        Name = "id",
                        Description = "User id"
                    }),
                resolve: async x => await userRepository.FindByIdAsync(x.GetArgument<string>("id"))
                );

            FieldAsync<UserType>(
                "findUserByUsername",
                description: "Find user by username",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>>()
                    {
                        Name = "username",
                        Description = "User account name"
                    }),
                resolve: async x => await userRepository.FindByUsernameAsync(x.GetArgument<string>("username"))
                );
        }


        private readonly IUserRepository userRepository;
    }
}
