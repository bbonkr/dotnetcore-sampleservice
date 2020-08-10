using System.Collections.Generic;

namespace SampleServices.Entities
{
    public class User
    {
        public string Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public virtual IList<RefreshToken> RefreshTokens { get; set; }
    }
}
