namespace Identitymanager.ViewModels;

public class UserRolesViewModel
{
    public string UserId { get; set; }   
    public string Email { get; set; }
    public bool TwoFactorEnabled { get; set; }

    public string UserName { get; set;}

    public string FullName { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
    public List<string> Roles { get; set; }

}
