using System;
using System.Collections.Generic;
using System.Text;

using GraphQL.Types;

using SampleService.Services.Schema;

namespace SampleService.Services.GraphqlQueries
{
    public class UserQuery:ObjectGraphType<object>
    {
        public UserQuery(IUserDataService userService)
        {
            this.userService = userService;

            FieldAsync<ListGraphType<UserType>>("users",
                description: "Get list of user",
                resolve: async c => await userService.GetAllAsync(_ => true, false));

            FieldAsync<UserType>(
                "findUserById",
                description: "Find user by id",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>>()
                    {
                        Name = "id",
                        Description = "User id"
                    }),
                resolve: async x => await userService.FindByIdAsync(x.GetArgument<string>("id"))
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
                resolve: async x => await userService.FindByUsernameAsync(x.GetArgument<string>("username"))
                );
        }


        private readonly IUserDataService userService;
    }
}
