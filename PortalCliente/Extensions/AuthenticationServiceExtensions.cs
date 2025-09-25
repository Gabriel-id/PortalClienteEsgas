using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace PortalCliente.Extensions
{
    public static class AuthenticationServiceExtensions
    {
        public static IServiceCollection AddSecureAuthentication(this IServiceCollection services)
        {
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.AccessDeniedPath = "/Account/AccessDenied";
                    options.ExpireTimeSpan = TimeSpan.FromHours(1);

                    // Configurações básicas de cookie
                    options.Cookie.HttpOnly = true; // Previne acesso via JavaScript (XSS)
                    options.Cookie.Name = "PortalClienteAuth"; // Nome customizado
                    options.Cookie.SameSite = SameSiteMode.Strict; // Proteção contra CSRF
                    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // HTTPS em produção, permite HTTP em dev
                    options.SlidingExpiration = true; // Renova o cookie automaticamente

                    // Validação de sessão
                    options.Events.OnValidatePrincipal = async context =>
                    {
                        var loginTimeClaim = context.Principal?.FindFirst("LoginTime");
                        if (loginTimeClaim != null && DateTime.TryParse(loginTimeClaim.Value, out var loginTime))
                        {
                            var sessionAge = DateTime.UtcNow - loginTime;

                            // Se passou muito tempo, invalida a sessão
                            if (sessionAge.TotalHours > 12)
                            {
                                context.RejectPrincipal();
                                await context.HttpContext.SignOutAsync();
                            }
                        }
                    };
                });

            return services;
        }
    }
}