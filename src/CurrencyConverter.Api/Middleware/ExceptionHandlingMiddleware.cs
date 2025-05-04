using System.Net;
using System.Text.Json;

namespace CurrencyConverter.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            // log exception with stack trace
            _logger.LogError(ex, "Unhandled exception for request {Method} {Path}",
                context.Request.Method, context.Request.Path);

            // prepare error response
            context.Response.Clear();
            context.Response.ContentType = "application/json";

            var status = ex switch
            {
                KeyNotFoundException => HttpStatusCode.NotFound,
                UnauthorizedAccessException => HttpStatusCode.Unauthorized,
                _ => HttpStatusCode.InternalServerError
            };
            context.Response.StatusCode = (int)status;

            var error = new
            {
                error = ex.Message,
            };
            var payload = JsonSerializer.Serialize(error);
            await context.Response.WriteAsync(payload);
        }
    }
}
