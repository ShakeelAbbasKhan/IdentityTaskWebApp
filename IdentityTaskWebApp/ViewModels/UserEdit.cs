using System.ComponentModel.DataAnnotations;

namespace IdentityTaskWebApp.ViewModels
{
    public class UserEdit
    {
        [Required]
        public string Id { get; set; }
        [Required]
        public string FirstName { get; set; }
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "The password and Confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
