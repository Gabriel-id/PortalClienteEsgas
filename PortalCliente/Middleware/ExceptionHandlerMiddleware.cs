using System.Diagnostics;
using System.Net;
using System.Text.Json;

namespace PortalCliente.Middleware;

public class ExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlerMiddleware> _logger;

    public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred while processing the request");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        string toastMessage;

        switch (exception)
        {
            case HttpRequestException:
                response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                toastMessage = "Serviço temporariamente indisponível. Tente novamente em alguns momentos.";
                break;
            case UnauthorizedAccessException:
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                toastMessage = "Acesso não autorizado. Faça login novamente.";
                break;
            case ArgumentException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                toastMessage = "Parâmetros inválidos na solicitação.";
                break;
            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                toastMessage = "Ocorreu um erro interno. Tente novamente ou entre em contato com o suporte.";
                break;
        }

        if (context.Request.Headers.XRequestedWith == "XMLHttpRequest")
        {
            context.Response.ContentType = "application/json";
            var errorResponse = new ErrorResponse
            {
                Message = toastMessage,
                StatusCode = response.StatusCode
            };
            var jsonResponse = JsonSerializer.Serialize(errorResponse);
            await context.Response.WriteAsync(jsonResponse);
        }
        else
        {
            response.Redirect("/Home/Error");
        }
    }
}

public class ErrorResponse
{
    public string Message { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public string TraceId { get; set; } = Activity.Current?.Id ?? string.Empty;
}