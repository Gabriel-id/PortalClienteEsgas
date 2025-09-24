using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using PortalCliente.Core.Dtos;
using PortalCliente.Core.Interfaces.Services;
using PortalCliente.Filters;
using System.Security.Claims;

namespace PortalCliente.Controllers
{
    public class AccountController(IAuthService authService, IValidator<Login> loginValidator) : BaseController
    {
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(Login login)
        {
            // Manual validation using FluentValidation
            var validationResult = await loginValidator.ValidateAsync(login);
            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                {
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }
                SendToastMessage("Please correct the validation errors", "error");
                return View(login);
            }

            try
            {
                var loginResult = await authService.Authenticate(login);

                if (!string.IsNullOrEmpty(loginResult.Token))
                {
                    var claims = new List<Claim>
                    {
                        new("UserName", loginResult.ClientName),
                        new("ClientCode", loginResult.Token)
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                    return RedirectToAction("Index", "Invoices");
                }

                SendToastMessage("Invalid credentials, please check and try again", "error");
            }
            catch (Exception)
            {
                SendToastMessage("An error occurred during login, please try again", "error");
            }

            return View(login);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }
    }
}
