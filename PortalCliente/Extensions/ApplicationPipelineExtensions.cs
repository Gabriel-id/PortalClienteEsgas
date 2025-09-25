using PortalCliente.Middleware;
using PortalCliente.Services;

namespace PortalCliente.Extensions
{
    public static class ApplicationPipelineExtensions
    {
        public static WebApplication ConfigurePipeline(this WebApplication app)
        {
            // Configure MockSapServer for Development
            ConfigureMockServer(app);

            // Global Exception Handling
            app.UseMiddleware<ExceptionHandlerMiddleware>();

            // Configure the HTTP request pipeline
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseStatusCodePagesWithReExecute("/Home/NotFound", "?statusCode={0}");
                app.UseHsts();
                app.UseSecurityHeaders();
            }
            else
            {
                app.UseStatusCodePagesWithReExecute("/Home/NotFound", "?statusCode={0}");
            }

            // Standard Pipeline
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            // Authentication & Authorization
            app.UseAuthentication();
            app.UseAuthorization();

            // Route Mapping
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            return app;
        }

        private static void ConfigureMockServer(WebApplication app)
        {
            MockSapServer? mockServer = null;

            if (app.Environment.IsDevelopment())
            {
                var useMock = app.Configuration.GetValue<bool>("SapService:UseMock", false);
                if (useMock)
                {
                    mockServer = new MockSapServer(app.Services.GetRequiredService<ILogger<MockSapServer>>());
                    mockServer.Start(8080);

                    // Ensure MockServer is disposed on shutdown
                    app.Lifetime.ApplicationStopping.Register(() => mockServer.Dispose());
                }
            }
        }
    }
}