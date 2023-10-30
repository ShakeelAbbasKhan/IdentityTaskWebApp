using System.ComponentModel.DataAnnotations;

namespace IdentityTaskWebApp.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
