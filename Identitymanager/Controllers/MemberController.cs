using Identitymanager.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;

namespace Identitymanager.Controllers;

public class MemberController : Controller
{
    private readonly UserManager<ApplicationUsers> _usermanager;
    private readonly ApplicationDbContext _db;

    public MemberController(UserManager<ApplicationUsers> userManager,ApplicationDbContext db)
    {
        _usermanager = userManager;
        _db = db;















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
}
