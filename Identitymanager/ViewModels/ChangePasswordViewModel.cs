using System.ComponentModel.DataAnnotations;

namespace Identitymanager.ViewModels;

public class ChangePasswordViewModel
{
    [Required(ErrorMessage ="Please enter an old passpord.")]
    [Display(Name ="Old Password")]
    [DataType(DataType.Password)]
    public string OldPassword { get; set; }
    [Required(ErrorMessage ="Please enter a new password.")]
    [Display(Name = "New Password")]
    [DataType(DataType.Password)]
    [StringLength(16,ErrorMessage ="Please enter a password between 8-16 digits",MinimumLength =8)]
    public string NewPassword { get; set; }
    [Required(ErrorMessage = "Please enter a new password again.")]
    [DataType(DataType.Password)]
    [Compare("NewPassword",ErrorMessage ="Password does not match with a new password.")]
    public string ConfirmPassword { get; set; }
}
