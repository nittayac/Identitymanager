using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Identitymanager.Controllers
{
    public class RoleController : Controller
    {
        private RoleManager<IdentityRole> _roleManager;

        public RoleController(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;

        }
        public IActionResult Index()
        {
            var rs = _roleManager.Roles.OrderBy(r => r.Name);
            return View(rs);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IdentityRole data)
        {
            if (ModelState.IsValid)
            {
                if (!await _roleManager.RoleExistsAsync(data.Name))
                {
                    var role = new IdentityRole(data.Name);
                    var roleresult = await _roleManager.CreateAsync(role);

                    if (roleresult.Succeeded)
                    {
                        return RedirectToAction("Index", "Role");
                    }
                }
                else
                {
                   // ModelState.AddModelError("", "This role is exist.");
                }
            }

            return View(data);
        }
    }
}
