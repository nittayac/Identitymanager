
using Identitymanager.Config;
using Identitymanager.Models;
using Identitymanager.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Runtime.InteropServices;
using Identitymanager.Services;
using System.Text.Encodings.Web;
using System.Security;
using System.Reflection.Metadata.Ecma335;

namespace Identitymanager.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUsers> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly SignInManager<ApplicationUsers> _signInManager;
    private readonly UrlEncoder _urlEncoder;

    public AccountController(UserManager<ApplicationUsers> userManager,
        RoleManager<IdentityRole> roleManager,
        SignInManager<ApplicationUsers> signInManager,
        UrlEncoder urlEncoder)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _signInManager = signInManager;
        _urlEncoder = urlEncoder;
    }
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel data) {

        if (ModelState.IsValid)
        {
            ApplicationUsers user = new ApplicationUsers();
            user.UserName = data.Email;
            user.Email = data.Email;
            user.ProvinceId = "HHHH";

            var result = await _userManager.CreateAsync(user,data.Password);
            if (result.Succeeded)
            {
                if (!await _roleManager.RoleExistsAsync(RoleName.Member))
                {
                    var role = new IdentityRole(RoleName.Admin);
                    var roleresult = await _roleManager.CreateAsync(role);
                }

                await _userManager.AddToRoleAsync(user, RoleName.Member);
                await _signInManager.SignInAsync(user, isPersistent: false);

                return RedirectToAction("Index", "Member");
            }
            else {
                AddError(result);
            }
        }
        return View();

    }

    public IActionResult Login() {

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel data) {

        if (ModelState.IsValid)
        {
            var result = await _signInManager.PasswordSignInAsync(
                data.Email.Trim(), data.Password.Trim(), false, false);

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Member");
            }
            else {
                data.Message = "Username and password are not correct.";
            }
            
            if (result.RequiresTwoFactor) 
            {
                return RedirectToAction(nameof(VerifyAuthenticatorCode));
            }

        }
        return View(data);

    }

    public async  Task<IActionResult> Logout() 
    {
        await _signInManager.SignOutAsync();

        return RedirectToAction("Index", "Home");
    }


    private void AddError(IdentityResult result)
    {

        foreach (var item in result.Errors)
        {
            ModelState.AddModelError("", item.Description);
        }
    }


    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotPassword()
    {
        return View();

    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel data)
    {
        if (ModelState.IsValid) {
            var users = await _userManager.FindByEmailAsync(data.Email);

            if (users == null)
            {
                return NotFound();
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(users);
            var callbackurl = Url.Action("ResetPassword", "Account",
                new {
                    userId = users.Id,
                    code = token
                }, protocol: HttpContext.Request.Scheme);

            var email = new EmailService();
            email.SendEmailAsync(data.Email, data.Email, callbackurl);

            return RedirectToAction("ForgotPasswordConfirmation");
        }

        return View(data);
    }

    [AllowAnonymous]
    public IActionResult ForgotPasswordConfirmation() {
        return View();
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ResetPassword(string code = null)
    {
        return code == null ? View("Error") : View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel data) {

        if (ModelState.IsValid)
        { 
            var user = await _userManager.FindByEmailAsync((string)data.Email);

            if (user == null) {
                return NotFound();
            }

            var result = await _userManager.ResetPasswordAsync(user, data.Code, data.Password);

            if (result.Succeeded) {
                return RedirectToAction("ResetPasswordConfirmation");
            }

            AddError(result);
        }

        return View(data);
    }


    [AllowAnonymous]
    public IActionResult ResetPasswordConfirmation() {
        return View();
    }

    public async Task<IActionResult> EnableAuthenucator()
    { 
        string uri = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";
        var user = await _userManager.GetUserAsync(User);
        await _userManager.ResetAuthenticatorKeyAsync(user);

        var token = await _userManager.GetAuthenticatorKeyAsync(user);
        string AuthentocatorUrl = string.Format(uri,
            _urlEncoder.Encode("identyityManager"),
            _urlEncoder.Encode(user.Email),token);

        var data = new TwoFactorAuthenticationViewModel()
        {
            Token = token,
            QRCodeUrl = AuthentocatorUrl
        };

        return View(data);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EnableAuthenucator(TwoFactorAuthenticationViewModel data)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.GetUserAsync(User);
            var result = await _userManager.VerifyTwoFactorTokenAsync(
                user,_userManager.Options.Tokens.AuthenticatorTokenProvider,data.Code
                );
            if (result)
            {
                await _userManager.SetTwoFactorEnabledAsync(user, true);
                await _signInManager.SignOutAsync();

                return RedirectToAction(nameof(AuthenticatorConfirm));
            }
            else 
            {
                ModelState.AddModelError("", "Something went wrong.");
            }
        }

        return View(data);
    }

    public  IActionResult AuthenticatorConfirm ()
    { 
        return View();
    
    }


    [AllowAnonymous]
    public async Task<IActionResult> VerifyAuthenticatorCode() 
    {
        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

        if (user == null)
        { 
            return View ("Error");
        }
        
        return View(new VerifyAuthenticatorCodeViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyAuthenticatorCode(VerifyAuthenticatorCodeViewModel data)
    {
        if (ModelState.IsValid)
        {
            var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(
                data.Code,
                isPersistent: false,
                rememberClient: false
                );

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Member");
            }
        }

        return View(data);
;   }
}
    
