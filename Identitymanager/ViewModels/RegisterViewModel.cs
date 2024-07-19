using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Identitymanager.ViewModels;

public class RegisterViewModel
{
    [Display(Name="Email:")]
    [Required(ErrorMessage ="Please enter an email")]
    public string Email { get; set; }

    [Display(Name = "Password:")]
    [Required(ErrorMessage = "Please enter a password.")]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [Display(Name = "Confirm Password:")]
    [Required(ErrorMessage = "Please enter a confirm password.")]
    [DataType(DataType.Password)]
    [Compare("Password",ErrorMessage ="Password does not match.")]
    public string PasswordAgain { get; set; }
}
