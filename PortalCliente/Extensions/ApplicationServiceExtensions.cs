using FluentValidation;
using Microsoft.Extensions.Options;
using PortalCliente.Core.Interfaces.Services;
using PortalCliente.Core.Services;
using PortalCliente.Infrastructure.Configuration;
using PortalCliente.Infrastructure.Services;

namespace PortalCliente.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // MVC Services
            services.AddControllersWithViews();

            // FluentValidation
            services.AddValidatorsFromAssemblyContaining<PortalCliente.Core.Validators.LoginValidator>();

            // SAP Service Configuration
            services.Configure<SapServiceOptions>(
                configuration.GetSection(SapServiceOptions.SectionName));

            // Configure HttpClient for SAP service
            services.AddHttpClient<ISapService, SapService>((serviceProvider, client) =>
            {
                var sapOptions = serviceProvider.GetRequiredService<IOptions<SapServiceOptions>>().Value;
                client.BaseAddress = new Uri(sapOptions.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(sapOptions.TimeoutSeconds);
            });

            // Application Services
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IInvoicesService, InvoicesService>();

            return services;
        }
    }
}