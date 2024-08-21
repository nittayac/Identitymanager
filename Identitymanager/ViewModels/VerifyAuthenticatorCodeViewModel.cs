using Microsoft.AspNetCore.Antiforgery;
using System.ComponentModel.DataAnnotations;

namespace Identitymanager.ViewModels;

public class VerifyAuthenticatorCodeViewModel
{
    [Required]
    public string Code { get; set; }
}
