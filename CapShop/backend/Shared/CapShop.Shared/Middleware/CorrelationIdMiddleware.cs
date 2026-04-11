using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CapShop.Shared.Middleware;

/// <summary>
/// Reads X-Correlation-ID from the incoming request header or generates a new one,
/// stores it in HttpContext.Items, and echoes it back on the response.
/// All subsequent ILogger calls in the same request automatically carry it via the log scope.
/// </summary>
public class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-ID";
    public const string ItemsKey = "CorrelationId";

    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers.TryGetValue(HeaderName, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value.ToString()
            : Guid.NewGuid().ToString("N");

        context.Items[ItemsKey] = correlationId;
        context.Response.Headers[HeaderName] = correlationId;

        using (_logger.BeginScope(new Dictionary<string, object> { [ItemsKey] = correlationId }))
        {
            await _next(context);
        }
    }
}
