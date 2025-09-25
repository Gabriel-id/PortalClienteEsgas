using Microsoft.AspNetCore.Mvc;

namespace PortalCliente.Controllers;

public class HomeController : BaseController
{
    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Invoices");

        return RedirectToAction("Login", "Account");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult NotFound(int? statusCode = null)
    {
        if (statusCode.HasValue)
            Response.StatusCode = statusCode.Value;
        else
            Response.StatusCode = 404;

        return Response.StatusCode switch
        {
            404 => View("NotFound"),
            _ => View("Error"),
        };
    }
}