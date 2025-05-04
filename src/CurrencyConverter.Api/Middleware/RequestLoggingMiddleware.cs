using System.Diagnostics;
using Serilog.Context;

namespace CurrencyConverter.Api.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;
    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        var sw = Stopwatch.StartNew();
        // Extract context
        var method = context.Request.Method;
        var path = context.Request.Path;
        var clientIp = context.Connection.RemoteIpAddress?.ToString();
        var clientId = context.User.FindFirst("client_id")?.Value ?? "anonymous";
        var traceId = Activity.Current?.TraceId.ToString() ?? "no-trace";

        // Push into Serilog Context
        using (LogContext.PushProperty("ClientIp", clientIp))
        using (LogContext.PushProperty("ClientId", clientId))
        using (LogContext.PushProperty("TraceId", traceId))
        {
            _logger.LogInformation("Starting request {Method} {Path}", method, path);

            await _next(context);

            sw.Stop();
            var status = context.Response.StatusCode;
            _logger.LogInformation(
                "Finished {Method} {Path} with {StatusCode} in {Elapsed:0.000} ms",
                method, path, status, sw.Elapsed.TotalMilliseconds);
        }
    }
}
