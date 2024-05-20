using System.ComponentModel.DataAnnotations;

namespace WorldCitiesAPI.Data
{
    public class NewPasswordRequest
    {
        [Required(ErrorMessage = "Email is required.")]
        public string Email { get; set; } = null!;
        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; } = null!;
        [Required(ErrorMessage = "Token is required.")]
        public string Token { get; set; } = null!;
    }
}
