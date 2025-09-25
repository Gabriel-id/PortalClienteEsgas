namespace PortalCliente.Extensions
{
    public static class SecurityHeadersExtensions
    {
        public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
        {
            app.Use(async (context, next) =>
            {
                // Headers de segurança para produção
                AddSecurityHeaders(context);
                await next();
            });

            return app;
        }

        private static void AddSecurityHeaders(HttpContext context)
        {
            // Previne clickjacking
            context.Response.Headers.XFrameOptions = "SAMEORIGIN";

            // Previne MIME type sniffing
            context.Response.Headers.XContentTypeOptions = "nosniff";

            // Força HTTPS
            context.Response.Headers.StrictTransportSecurity = "max-age=31536000; includeSubDomains";

            // Referrer policy
            context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

            // CSP básico para prevenir XSS
            context.Response.Headers.ContentSecurityPolicy =
                "default-src 'self'; " +
                "script-src 'self' 'unsafe-inline' https://ajax.googleapis.com https://maxcdn.bootstrapcdn.com; " +
                "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com; " +
                "font-src 'self' https://fonts.gstatic.com; " +
                "img-src 'self' data:;";

            // Remove header que revela versão do servidor
            context.Response.Headers.Remove("Server");
            context.Response.Headers.Remove("X-Powered-By");
        }
    }
}