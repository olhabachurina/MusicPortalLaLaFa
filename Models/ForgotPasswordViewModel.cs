using System.ComponentModel.DataAnnotations;
namespace MusicPortalLaLaFa.Models
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
