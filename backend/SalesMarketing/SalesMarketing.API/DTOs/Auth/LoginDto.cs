using System.ComponentModel.DataAnnotations;

namespace SalesMarketing.API.DTOs.Auth
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(200)]
        public string Password { get; set; } = string.Empty;
    }
}
