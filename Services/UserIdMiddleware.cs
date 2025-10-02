namespace ChatBotLamaApi.Services
{
    public class UserIdMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public UserIdMiddleware(RequestDelegate next, ILogger logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Request.Cookies.ContainsKey("user_id"))
            {
                var userId = Guid.NewGuid().ToString();
                context.Response.Cookies.Append("user_id", userId,
                    new CookieOptions
                    {
                        Expires = DateTimeOffset.UtcNow.AddDays(30),
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict
                    });
                _logger.LogInformation($"User key: {userId}");
            }

            await _next(context);
        }
    }
}
