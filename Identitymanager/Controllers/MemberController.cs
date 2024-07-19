using Identitymanager.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Identitymanager.Controllers;

public class MemberController : Controller
{
    private readonly UserManager<ApplicationUsers> _usermanager;

    public MemberController(UserManager<ApplicationUsers> userManager)
    {
        _usermanager = userManager;
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
}
