using ChatBotLamaApi.Interfaces;

namespace ChatBotLamaApi.Services
{
    public class UserIdMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<UserIdMiddleware> _logger;
        private readonly IRateLimiter _rateLimiter;

        public UserIdMiddleware(RequestDelegate next, ILogger<UserIdMiddleware> logger, IRateLimiter rateLimiter)
        {
            _rateLimiter = rateLimiter;
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            _logger.LogInformation($"Invoke is running");
            if (!context.Request.Cookies.TryGetValue("user_id", out var userId) || string.IsNullOrEmpty(userId))
            {
                var newUserId = await _rateLimiter.CreateSessionAsync();

                context.Response.Cookies.Append("user_id", newUserId,
                    new CookieOptions
                    {
                        Expires = DateTimeOffset.UtcNow.AddDays(1),
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict
                    });
                _logger.LogInformation($"New user session created: {newUserId}");

                _logger.LogInformation($"User key: {newUserId}");


            }
            else
            {
                var sessionValid = await _rateLimiter.ValidateSessionAsync(userId);

                if (!sessionValid)
                {
                    _logger.LogWarning($"Session not found in Redis for user: {userId}");

                    var newUserId = await _rateLimiter.CreateSessionAsync();
                    context.Response.Cookies.Append("user_id", newUserId,
                        new CookieOptions
                        {
                            Expires = DateTimeOffset.UtcNow.AddDays(1),
                            HttpOnly = true,
                            Secure = true,
                            SameSite = SameSiteMode.Strict
                        });

                    _logger.LogInformation($"Created new session for expired user: {newUserId}");
                }
                else
                {
                    _logger.LogInformation($"Valid session found for user: {userId}");


                }
            }
                _logger.LogInformation($"Cookies contains user_id: {context.Request.Cookies["user_id"]}");
            await _next(context);
        }
    }
}
