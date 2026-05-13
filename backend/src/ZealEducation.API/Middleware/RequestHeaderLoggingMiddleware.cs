namespace ZealEducation.API.Middleware;

public class RequestHeaderLoggingMiddleware(
    RequestDelegate next,
    ILogger<RequestHeaderLoggingMiddleware> logger)
{
    private static readonly HashSet<string> SensitiveHeaders = new(StringComparer.OrdinalIgnoreCase)
    {
        "Authorization",
        "Cookie",
        "Set-Cookie",
        "Proxy-Authorization"
    };

    public Task InvokeAsync(HttpContext context)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            var headers = context.Request.Headers
                .Select(h => $"{h.Key}: {(SensitiveHeaders.Contains(h.Key) ? "***REDACTED***" : h.Value.ToString())}");

            logger.LogInformation(
                "HTTP {Method} {Path}{QueryString} headers: {Headers}",
                context.Request.Method,
                context.Request.Path,
                context.Request.QueryString,
                string.Join("; ", headers));
        }

        return next(context);
    }
}
