using ChatBotLamaApi.Interfaces;
using ChatBotLamaApi.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace ChatBotLamaApi.Handlers
{
    public class CookieAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IRateLimiter _ratelimiter;
        private readonly ILogger<CookieAuthenticationHandler> _logger;

        public CookieAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IRateLimiter rateLimiter) : base(options, logger, encoder, clock)
        {
            _logger = logger.CreateLogger<CookieAuthenticationHandler>();
            _ratelimiter = rateLimiter;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            _logger.LogInformation("CookieAuthenticationHandler is running");


            if (!Request.Cookies.TryGetValue("user_id", out var userId) || string.IsNullOrEmpty(userId))
            {
                return AuthenticateResult.Fail("User ID not found in cookies");
            }

            var redisKey = $"session:{userId}:requests_left";



            var sessionExists = await _ratelimiter.SessionExistsAsync(userId);
            if (!sessionExists)
            {
                return AuthenticateResult.Fail("Session not found in Redis");
            }


            _logger.LogInformation($"Authorizing user: {userId}");
            var claims = new[] {
            new Claim(ClaimTypes.Name, userId),
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim("UserID", userId),
            new Claim("SessionType", "RedisCookie"),
            new Claim("AuthTime", DateTime.UtcNow.ToString("O"))
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            _logger.LogInformation($"User {userId} successfully authorized");

            return AuthenticateResult.Success(ticket);
        }
    }
}
