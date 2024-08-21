using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Identitymanager.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage ="Please enter an email.")]
    [Display(Name ="E-mail")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Please enter a password.")]
    [Display(Name ="Password")]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [BindProperty]//[ViewData]
    public string Message { get; set; }

    [BindProperty]//[ViewData]
    public string LoginError { get; set; }

}
