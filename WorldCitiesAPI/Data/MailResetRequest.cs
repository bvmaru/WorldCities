using System.ComponentModel.DataAnnotations;

namespace WorldCitiesAPI.Data
{
    public class MailResetRequest
    {
        [Required(ErrorMessage = "Email is required.")]
        public string Email { get; set; } = null!;
    }
}
