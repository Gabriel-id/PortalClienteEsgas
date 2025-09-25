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
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Invoices");
            }

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
                SendToastMessage("Por favor, corrija os erros de validação", "error");
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
                        new("ClientCode", loginResult.Token),
                        new("LoginTime", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"))
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                    return RedirectToAction("Index", "Invoices");
                }

                SendToastMessage("Credenciais inválidas, por favor verifique e tente novamente", "error");
            }
            catch (Exception)
            {
                SendToastMessage("Ocorreu um erro durante o login, por favor tente novamente", "error");
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
