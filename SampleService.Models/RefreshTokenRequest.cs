using System.ComponentModel.DataAnnotations;

namespace SampleService.Models
{
    public class RefreshTokenRequest
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Token is required")]
        public string Token { get; set; }
    }
}
