namespace AppService.Api.Middleware;

public class ApiKeyAuthenticationMiddleware(RequestDelegate next, ILogger<ApiKeyAuthenticationMiddleware> logger)
{
    private const string ApiKeyHeaderName = "X-API-KEY";
    private const string ValidApiKey = "seller-secret-key-12345";

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        // Skip authentication for health check and swagger
        if (path.StartsWith("/health") || path.StartsWith("/swagger") || path.StartsWith("/_"))
        {
            await next(context);
            return;
        }

        // Check API key for /api/ routes
        if (path.StartsWith("/api/"))
        {
            if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var apiKey))
            {
                logger.LogWarning("Missing API key for request to {Path}", path);
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { error = "Unauthorized: Missing API key" });
                return;
            }

            if (apiKey != ValidApiKey)
            {
                logger.LogWarning("Invalid API key for request to {Path}", path);
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { error = "Unauthorized: Invalid API key" });
                return;
            }
        }

        await next(context);
    }
}