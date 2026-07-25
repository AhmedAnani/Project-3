using CountryExplorer.Api.Middlewares;

namespace CountryExplorer.Api.Middlewares;

public static class MiddlewareExtensions
{

    public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder app)
        => app.UseMiddleware<RateLimitingMiddleware>();
}
