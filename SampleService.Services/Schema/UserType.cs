using System;
using System.Collections.Generic;
using System.Text;

using GraphQL.Types;

using SampleService.Entities;

namespace SampleService.Services.Schema
{
    public class UserType: ObjectGraphType<User>
    {
        public UserType()
        {
            this.Field(x => x.Id);
            this.Field(x => x.UserName);
            this.Field(x => x.LastName);
            this.Field(x => x.FirstName);
        }
    }
}
