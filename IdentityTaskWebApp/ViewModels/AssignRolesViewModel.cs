namespace IdentityTaskWebApp.ViewModels
{
    public class AssignRolesViewModel
    {
        public string UserId { get; set; }
        public string UserEmail { get; set; }
        public List<UserRoleViewModel> Roles { get; set; }
    }
}
