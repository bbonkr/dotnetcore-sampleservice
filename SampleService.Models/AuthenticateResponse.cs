using System;
using System.Collections.Generic;
using System.Text;

namespace SampleService.Models
{
   public class AuthenticateResponse
    {
        public string Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string UserName { get; set; }

        public string JwtToken { get; set; }

        public string RefreshToken { get; set; }

        public AuthenticateResponse()
        {

        }
    }
}
