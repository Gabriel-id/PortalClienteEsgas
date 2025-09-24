using FluentValidation;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using PortalCliente.Core.Interfaces.Services;
using PortalCliente.Core.Services;
using PortalCliente.Infrastructure.Services;
using PortalCliente.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/portal-cliente-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(1);
    });

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<PortalCliente.Core.Validators.LoginValidator>();

// Configure SAP Service options
builder.Services.Configure<PortalCliente.Core.Configuration.SapServiceOptions>(
    builder.Configuration.GetSection(PortalCliente.Core.Configuration.SapServiceOptions.SectionName));

// Configure HttpClient for SAP service
builder.Services.AddHttpClient<ISapService, SapService>((serviceProvider, client) =>
{
    var sapOptions = serviceProvider.GetRequiredService<IOptions<PortalCliente.Core.Configuration.SapServiceOptions>>().Value;
    client.BaseAddress = new Uri(sapOptions.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(sapOptions.TimeoutSeconds);
});

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IInvoicesService, InvoicesService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
