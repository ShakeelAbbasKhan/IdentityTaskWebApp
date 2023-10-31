namespace IdentityTaskWebApp.ViewModels
{
    public class UserListVM
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public List<string> Roles { get; set; } // Property to store roles

    }
}
