using Azure.Core;
using Identitymanager.Config;
using Identitymanager.Models;
using Identitymanager.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;

namespace Identitymanager.Controllers;

public class AdminController : Controller
{
    private readonly UserManager<ApplicationUsers> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AdminController(UserManager<ApplicationUsers> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }
    public async Task<IActionResult> Index()
    {
        var users = await _userManager.Users.ToListAsync();
        var viewmodel = new List<UserRolesViewModel>();

        foreach (var user in users)
        {
            var vm = new UserRolesViewModel();
            vm.UserId = user.Id;
            vm.Email = user.Email;
            vm.TwoFactorEnabled = user.TwoFactorEnabled;
            vm.UserName = user.UserName;
            vm.FullName = user.FullName;
            vm.LockoutEnd = user.LockoutEnd;
            vm.Roles = await GetRoleList(user);

            viewmodel.Add(vm);
        }    

        return View(viewmodel);
    }

    private async Task<List<string>> GetRoleList(ApplicationUsers user)
    {
        return new List<string>(await _userManager.GetRolesAsync(user));
    }

    public IActionResult Dashboard()
    {
        return View();
    }


    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RegisterViewModel data)
    {
        if (ModelState.IsValid)
        {
            ApplicationUsers user = new ApplicationUsers();
            user.UserName = data.Email.Trim();
            user.Email = data.Email.Trim();
            user.FullName = "";
            user.Address = "";
            user.ImgUrl = "";

            var result = await _userManager.CreateAsync(user, data.Password);
            if (result.Succeeded)
            {
                if (!await _roleManager.RoleExistsAsync(RoleName.Member))
                {
                    var role = new IdentityRole(RoleName.Member);
                    var roleresult = await _roleManager.CreateAsync(role);
                }

                await _userManager.AddToRoleAsync(user, RoleName.Member);
                return RedirectToAction("Index", "Admin");
            }
            else
            {
                foreach (var item in result.Errors)
                {
                    ModelState.AddModelError("", item.Description);
                }
            }
        }
        return View(data);
    
    }

    public IActionResult AddRoleToUser(string id)
    {
        ApplicationUsers user = _userManager.FindByIdAsync(id).Result;

        var viewmodel = new AddRoleToUserViewModel
        {
            UserId = user.Id,
            Email = user.Email,
            Roles = GetAllRoles()
        };

        ViewData["NewRole"] = new SelectList(_roleManager.Roles.OrderBy(r => r.Name));

        return View(viewmodel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddRoleToUser(AddRoleToUserViewModel data) 
    {
        ApplicationUsers user = _userManager.FindByIdAsync (data.UserId).Result;

        if (ModelState.IsValid)
        {
            if (!await _userManager.IsInRoleAsync(user, data.NewRole))
            {
                var result = await _userManager.AddToRoleAsync(user,data.NewRole);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Admin");
                }
                else
                {
                    foreach (var item in result.Errors)
                    {
                        ModelState.AddModelError("", item.Description);
                    }
                }
            }
            else
            {
                ModelState.AddModelError("", "User has this role already.");
            }
        }
        ViewData["NewRole"] = new SelectList(_roleManager.Roles.OrderBy(r => r.Name),data.NewRole);
        return View(data);
    }

    public async Task<IActionResult>RemoveRoleFromUser(string id)
    {
        ApplicationUsers user = _userManager.FindByIdAsync(id).Result;
        var userRoles = string.Join(",", await _userManager.GetRolesAsync(user));

        var viewmodel = new RemoveRoleFromUserViewModel 
        {
            UserId = user.Id,
            Email = user.Email,
            Roles = userRoles
        };
        
        return View(viewmodel);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveRoleFromUser(RemoveRoleFromUserViewModel data)
    {
        ApplicationUsers user = _userManager.FindByIdAsync(data.UserId).Result;
        var userRoles =await _userManager.GetRolesAsync(user);

        if (ModelState.IsValid)
        {
            if (userRoles.Count > 0)
            {
                foreach (var role in userRoles.ToList())
                {
                    await _userManager.RemoveFromRoleAsync(user, role);
                }

              return  RedirectToAction("Index", "Admin");
            }
        }

        return View(data);
    }
    private SelectList GetAllRoles()
    {
        return new SelectList(_roleManager.Roles.OrderBy(r=> r.Name));
    }
}
