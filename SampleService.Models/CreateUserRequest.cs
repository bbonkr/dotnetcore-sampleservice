using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SampleService.Models
{
    public class CreateUserRequest
    {
        /// <summary>
        /// 이름
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "First name is required")]
        public string FirstName { get; set; }

        /// <summary>
        /// 이름
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "Last name is required")]
        public string LastName { get; set; }

        /// <summary>
        /// 사용자 계정 이름
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "User name is required")]
        public string UserName { get; set; }

        /// <summary>
        /// 비밀번호
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "Password is required")]
        public string Password { get; set; }
    }

    public class CreateUserResponse : AppResponse { }

    public class UpdateUserRequest
    {
        /// <summary>
        /// 사용자 계정 이름
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "Username is required")]
        public string UserName { get; set; }

        /// <summary>
        /// 이름
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "First name is required")]
        public string FirstName { get; set; }

        /// <summary>
        /// 이름
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "Last name is required")]
        public string LastName { get; set; }
    }

    public class UpdateUserResponse : AppResponse { }

    public class ChangePasswordRequest
    {
        /// <summary>
        /// 사용자 계정 이름
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "Username is required")]
        public string UserName { get; set; }

        /// <summary>
        /// 현재 비밀번호
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "Current password is required")]
        public string CurrentPassword { get; set; }

        /// <summary>
        /// 변경할 비밀번호
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "Password is required")]
        public string Password { get; set; }
    }

    public class ChangePasswordResponse : AppResponse { }

    public class CloseAccountRequest
    {
        /// <summary>
        /// 사용자 계정 이름
        /// </summary>
        [Required(AllowEmptyStrings =false, ErrorMessage ="Username is required")]
        public string UserName { get; set; }
    }

    public class CloseAccountResponse : AppResponse { }
}
