using System.ComponentModel.DataAnnotations;

namespace Identitymanager.ViewModels;

public class ResetPasswordViewModel
{
    [Required(ErrorMessage ="Please enter an email.")]
    [EmailAddress]
    [Display(Name ="E-mail")]
    public  string Email { get; set; }

    [Required(ErrorMessage = "Please enter a new password.")]
    [DataType(DataType.Password)]
    [StringLength(16,ErrorMessage ="Password should be 8-16 characters",MinimumLength =8)]
    [Display(Name = "Password")]
    public  string Password { get; set; }


    [Required(ErrorMessage = "Please enter a confirm new password.")]
    [DataType(DataType.Password)]
    [Compare("Password",ErrorMessage ="Passwords are not the same.")]
    [Display(Name = "ConfirmPassword")]
    public string ConfirmPassword { get; set; }

    public string Code { get; set; }
}
