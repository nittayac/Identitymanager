
using Identitymanager.Config;
using Identitymanager.Models;
using Identitymanager.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Runtime.InteropServices;
using Identitymanager.Services;

namespace Identitymanager.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUsers> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly SignInManager<ApplicationUsers> _signInManager;

    public AccountController(UserManager<ApplicationUsers> userManager,
        RoleManager<IdentityRole> roleManager,
        SignInManager<ApplicationUsers> signInManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _signInManager = signInManager;
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

            if (result.Succeeded) {
                return RedirectToAction("Index", "Member");
            }


        }
        return View(data);

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
}
