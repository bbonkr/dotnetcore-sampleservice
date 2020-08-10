using System;

namespace SampleServices.Entities
{
    public class RefreshToken
    {
        public string Id { get; set; }
        

        public string UserId { get; set; }

        public virtual User User { get; set; }

        public string Token { get; set; }

        public DateTimeOffset Expires { get; set; }

        public DateTimeOffset Created { get; set; }

        public string CreatedByIp { get; set; }

        public DateTimeOffset? Revoked { get; set; }

        public string RevokedByIp { get; set; }

        public string ReplacedByToken { get; set; }

        public bool IsExpired => DateTimeOffset.UtcNow >= Expires;

        public bool IsActive => Revoked == null && !IsExpired;
    }
}
