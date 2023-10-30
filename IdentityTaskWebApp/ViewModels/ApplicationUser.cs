using Microsoft.AspNetCore.Identity;

namespace IdentityTaskWebApp.ViewModels
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }
}
