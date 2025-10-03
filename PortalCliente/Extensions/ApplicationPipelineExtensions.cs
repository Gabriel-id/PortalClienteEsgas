using PortalCliente.Middleware;

namespace PortalCliente.Extensions
{
    public static class ApplicationPipelineExtensions
    {
        public static WebApplication ConfigurePipeline(this WebApplication app)
        {
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
    }
}