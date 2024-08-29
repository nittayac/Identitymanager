using Identitymanager.Config;
using Identitymanager.Models;
using Identitymanager.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using System.Reflection.Metadata.Ecma335;

namespace Identitymanager.Controllers;

public class MemberController : Controller
{
    private readonly UserManager<ApplicationUsers> _usermanager;
    private readonly ApplicationDbContext _db;
    private readonly SignInManager<ApplicationUsers> _signInManager;

    public MemberController(UserManager<ApplicationUsers> userManager,
                                    ApplicationDbContext db,
                                    SignInManager<ApplicationUsers> signInManager)
    {
        _usermanager = userManager;
        _db = db;
        _signInManager = signInManager;
    }
    public IActionResult Index()
    {
        if(User.Identity.IsAuthenticated)
        {
            var userId = _usermanager.GetUserId(HttpContext.User);
            ApplicationUsers user =_usermanager.FindByIdAsync(userId).Result;

            if (user == null)
            {
                return NotFound();
            }
            else { 
                return View(user);
            }

        }
        else{
            return RedirectToAction("Index", "Account");
        }
      
    }

    public IActionResult Edit(string Id)
    {
        var userId = _usermanager.GetUserId(HttpContext.User);
        ApplicationUsers user = _usermanager.FindByIdAsync(userId).Result;

        if (user == null)
        {
            return NotFound();
        }

        return View(user);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public  async Task<IActionResult> Edit(string Id, ApplicationUsers data, IFormFile files)
    {
        if (Id != data.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            var userStore = new UserStore<ApplicationUsers>(_db);
            var userId = _usermanager.GetUserId(HttpContext.User);
            ApplicationUsers user = _usermanager.FindByIdAsync(userId).Result;

            if (user != null)
            {
                if (files != null)
                {
                    if (files.Length > 0)
                    {
                        var filename = Path.GetFileName(files.FileName);
                        var fileExt = Path.GetExtension(files.FileName);

                        var tmpName = Guid.NewGuid().ToString();
                        var newFileName = string.Concat(tmpName, fileExt);
                        var filepath = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img")).Root + $@"\{newFileName}";

                        using (FileStream fs = System.IO.File.Create(filepath))
                        { 
                            files.CopyTo(fs);
                            fs.Flush();
                        }

                        user.ImgUrl = newFileName.Trim();
                    }
                }

                user.FullName = data.FullName;
                user.Address = data.Address;
                user.BirthDate = data.BirthDate;
                user.PhoneNumber = data.PhoneNumber;

                var result = await _usermanager.UpdateAsync(user);
                var ctx = userStore.Context;
                await ctx.SaveChangesAsync();

                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Member");
                }
                else { 
                    foreach (var item in result.Errors)
                    {
                        ModelState.AddModelError("", item.Description);
                    }
                }
            }

            
        }

        return View(data);
    
    }

    public IActionResult ChangePassword()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel data)
    {
        if (ModelState.IsValid)
        { 
            var user = await _usermanager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound();
            }

            var result = await _usermanager.ChangePasswordAsync(user,data.OldPassword,data.NewPassword);
            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                return RedirectToAction("Index", "Member");
            }
            else {
                foreach (var item in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, item.Description);
                }
            }


        }

        return View(data);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RoleName.Admin)]
    public async Task<IActionResult> BanUnBan(string id)
    {
        ApplicationUsers user = await _usermanager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        if (user.LockoutEnd != null  && user.LockoutEnd > DateTime.Now) //unban
        {
            user.LockoutEnd = DateTime.Now;
        }
        else //ban
        { 
            user.LockoutEnd = DateTime.Now.AddYears(200);
        }

        await _usermanager.UpdateAsync(user);

        return RedirectToAction("Index", "Admin");
    }
}
