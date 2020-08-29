using System.Collections.Generic;

namespace SampleService.Entities
{
    public class User
    {
        /// <summary>
        /// 식별자
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 이름
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// 이름
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// 사용자 계정 이름
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 해시된 비밀번호
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 계정 잠금여부
        /// </summary>
        public bool IsLocked { get; set; }

        /// <summary>
        /// 사용여부
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 인증 시도 실패수
        /// </summary>
        public int FailCount { get; set; }

        public virtual IList<RefreshToken> RefreshTokens { get; set; }
    }
}
