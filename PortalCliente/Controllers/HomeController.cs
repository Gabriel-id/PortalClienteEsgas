using Microsoft.AspNetCore.Mvc;

namespace PortalCliente.Controllers;

public class HomeController : BaseController
{
    public IActionResult Index()
    {
        // If user is already authenticated, redirect to invoices
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Invoices");
        }

        // Otherwise, redirect to login
        return RedirectToAction("Login", "Account");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }
}