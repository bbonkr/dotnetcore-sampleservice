using System;
using System.Collections.Generic;
using System.Text;

using GraphQL;

using SampleService.Services.GraphqlQueries;

namespace SampleService.Services.Schema
{
    public class UserSchema : GraphQL.Types.Schema
    {
        public UserSchema(
            UserQuery query,
            IDependencyResolver resolver
            )
        {
            this.Query = query;
            this.DependencyResolver = resolver;
        }
    }
}
