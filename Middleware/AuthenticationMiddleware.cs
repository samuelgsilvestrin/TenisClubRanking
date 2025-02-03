using Microsoft.Extensions.Logging;

namespace TennisClubRanking.Middleware
{
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuthenticationMiddleware> _logger;
        private readonly string[] _allowedPaths = new[] 
        { 
            "/auth/login", 
            "/auth/register",
            "/lib",
            "/css",
            "/js",
            "/favicon.ico"
        };

        public AuthenticationMiddleware(RequestDelegate next, ILogger<AuthenticationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower() ?? "";
            
            // Log the current request path and session info
            _logger.LogInformation($"Request path: {path}");
            _logger.LogInformation($"Session UserId: {context.Session.GetInt32("UserId")}");
            _logger.LogInformation($"Session Username: {context.Session.GetString("Username")}");
            
            // Check if the path starts with any of the allowed paths
            bool isAllowedPath = _allowedPaths.Any(allowedPath => path.StartsWith(allowedPath));

            if (isAllowedPath)
            {
                _logger.LogInformation($"Allowing access to path: {path}");
                await _next(context);
                return;
            }

            // Check if user is authenticated
            var userId = context.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                _logger.LogWarning($"User not authenticated, redirecting to login from: {path}");
                context.Response.Redirect("/Auth/Login");
                return;
            }

            _logger.LogInformation($"User {userId.Value} authenticated, proceeding to: {path}");
            await _next(context);
        }
    }

    public static class AuthenticationMiddlewareExtensions
    {
        public static IApplicationBuilder UseAuthenticationMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthenticationMiddleware>();
        }
    }
}
