using System;
using System.Collections.Generic;
using System.Text;

namespace SampleService.Entities
{
    /// <summary>
    /// 인증로그
    /// </summary>
    public class AuthorizationLog
    {
        public AuthorizationLog()
        {
            CreatedAt = DateTimeOffset.UtcNow;
        }

        /// <summary>
        /// 식별자
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 사용자 계정이름
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// 성공여부
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 아이피주소
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// 기기명칭
        /// </summary>
        public string Hostname { get; set; }

        /// <summary>
        /// 작성시각
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; }
    }
}
