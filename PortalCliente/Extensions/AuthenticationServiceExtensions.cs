using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace PortalCliente.Extensions
{
    public static class AuthenticationServiceExtensions
    {
        public static IServiceCollection AddSecureAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var sessionExpirationHours = configuration.GetValue<int>("Authentication:SessionExpirationHours", 1);
            var slidingExpiration = configuration.GetValue<bool>("Authentication:SlidingExpiration", true);
            var maxSessionAgeHours = configuration.GetValue<int>("Authentication:MaxSessionAgeHours", 12);

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.AccessDeniedPath = "/Account/AccessDenied";
                    options.ExpireTimeSpan = TimeSpan.FromHours(sessionExpirationHours);
                    options.SlidingExpiration = slidingExpiration;

                    options.Cookie.HttpOnly = true;
                    options.Cookie.Name = "PortalClienteAuth";
                    options.Cookie.SameSite = SameSiteMode.Strict;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;


                    options.Events.OnValidatePrincipal = async context =>
                    {
                        var loginTimeClaim = context.Principal?.FindFirst("LoginTime");
                        if (loginTimeClaim != null && DateTime.TryParse(loginTimeClaim.Value, out var loginTime))
                        {
                            var sessionAge = DateTime.UtcNow - loginTime;

                            if (sessionAge.TotalHours > maxSessionAgeHours)
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