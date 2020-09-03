using System;

namespace SampleService.Entities
{
    /// <summary>
    /// 리프레시 토큰
    /// </summary>
    public class RefreshToken
    {
        public RefreshToken()
        {
            Created = DateTimeOffset.UtcNow;
        }

        /// <summary>
        /// 식별자
        /// </summary>
        public string Id { get; set; }
        
        /// <summary>
        /// 사용자 식별자
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// 사용자
        /// </summary>
        public virtual User User { get; set; }
        /// <summary>
        /// 토큰
        /// </summary>
        public string Token { get; set; }
        /// <summary>
        /// 만료시각
        /// </summary>
        public DateTimeOffset Expires { get; set; }
        /// <summary>
        /// 작성시각
        /// </summary>
        public DateTimeOffset Created { get; set; }
        /// <summary>
        /// 작성요청 아이피 주소
        /// </summary>
        public string CreatedByIp { get; set; }
        /// <summary>
        /// 취소시각
        /// </summary>
        public DateTimeOffset? Revoked { get; set; }
        /// <summary>
        /// 취소요청 아이피 주소
        /// </summary>
        public string RevokedByIp { get; set; }
        /// <summary>
        /// 취소요청 토큰
        /// </summary>
        public string ReplacedByToken { get; set; }
        /// <summary>
        /// 만료여부
        /// </summary>
        public bool IsExpired => DateTimeOffset.UtcNow >= Expires;
        /// <summary>
        /// 활성여부
        /// </summary>
        public bool IsActive => Revoked == null && !IsExpired;
    }
}
