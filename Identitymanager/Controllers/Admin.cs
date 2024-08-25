using Microsoft.AspNetCore.Mvc;

namespace Identitymanager.Controllers;

public class Admin : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Dashboard()
    {
        return View();
    }
}
