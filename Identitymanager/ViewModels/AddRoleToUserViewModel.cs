using Microsoft.AspNetCore.Mvc.Rendering;

namespace Identitymanager.ViewModels;

public class AddRoleToUserViewModel
{
    public string UserId { get; set; }
    public string Email { get; set; }
    public string NewRole { get; set; }
    public SelectList Roles { get; set; }
}
