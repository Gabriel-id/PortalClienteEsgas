using FluentValidation;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using PortalCliente.Core.Interfaces.Services;
using PortalCliente.Core.Services;
using PortalCliente.Infrastructure.Configuration;
using PortalCliente.Infrastructure.Services;
using PortalCliente.Middleware;
using PortalCliente.Services;
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
builder.Services.Configure<SapServiceOptions>(
    builder.Configuration.GetSection(SapServiceOptions.SectionName));

// Configure HttpClient for SAP service
builder.Services.AddHttpClient<ISapService, SapService>((serviceProvider, client) =>
{
    var sapOptions = serviceProvider.GetRequiredService<IOptions<SapServiceOptions>>().Value;
    client.BaseAddress = new Uri(sapOptions.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(sapOptions.TimeoutSeconds);
});

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IInvoicesService, InvoicesService>();

var app = builder.Build();

// Configure MockSapServer for Development
MockSapServer? mockServer = null;
if (app.Environment.IsDevelopment())
{
    var useMock = app.Configuration.GetValue<bool>("SapService:UseMock", false);
    if (useMock)
    {
        mockServer = new MockSapServer(app.Services.GetRequiredService<ILogger<MockSapServer>>());
        mockServer.Start(8080);
    }
}

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

// Ensure MockServer is disposed on shutdown
if (mockServer != null)
{
    app.Lifetime.ApplicationStopping.Register(() => mockServer.Dispose());
}

app.Run();
