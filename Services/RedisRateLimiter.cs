using ChatBotLamaApi.Interfaces;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using StackExchange.Redis;

namespace ChatBotLamaApi.Services
{
    public class RedisRateLimiter : IRateLimiter
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly ILogger<RedisRateLimiter> _logger;
        private readonly TimeSpan _sessionLifetime = TimeSpan.FromDays(1);

        public RedisRateLimiter(IConnectionMultiplexer redis, ILogger<RedisRateLimiter> logger)
        {
            _redis = redis;
            _logger = logger;
        }

        public async Task<string> CreateSessionAsync()
        {
            var db = _redis.GetDatabase();
            var userId = Guid.NewGuid().ToString();
            var key = $"session:{userId}";

    
            var values = new HashEntry[]
            {
                new("status", "active"),
                new("requests_left", 30),
                new("created_at", DateTime.UtcNow.ToString("O"))
            };

            await db.HashSetAsync(key, values);
            await db.KeyExpireAsync(key, TimeSpan.FromDays(1)); 

            return userId;
        }

        public async Task<bool> ValidateSessionAsync(string userId) //Extends TTL
        {
            var db = _redis.GetDatabase();
            var sessionKey = $"session:{userId}";


            var exists = await db.KeyExistsAsync(sessionKey);
            if (exists)
            {
                await db.KeyExpireAsync(sessionKey, _sessionLifetime);
                return true;
            }
            return false;
        }

        public async Task<bool> SessionExistsAsync(string userId) //checking the existence of a key
        {
            var db = _redis.GetDatabase();
            var sessionKey = $"session:{userId}";
            return await db.KeyExistsAsync(sessionKey);
        }

        public async Task<bool> TryConsumeRequestAsync(string userId) //query limit check
        {
            var db = _redis.GetDatabase();
            var sessionKey = $"session:{userId}";

            if (!await db.KeyExistsAsync(sessionKey))
                return false;

            var remaining = await db.HashDecrementAsync(sessionKey, "requests_left");
            _logger.LogInformation($"Requests left: {remaining}");
            if (remaining < 0)
            {
                _logger.LogInformation("Daily limit is over");
                await db.HashSetAsync(sessionKey, "requests_left", 0);
                return false; 
            }

            return true;
        }

        public async Task<int> GetRemainingRequestsAsync(string userId) //updating available queries
        {
            if (string.IsNullOrEmpty(userId))
                return 0;

            var db = _redis.GetDatabase();
            var sessionKey = $"session:{userId}";


            var value = await db.HashGetAsync(sessionKey, "requests_left");

            if (!value.HasValue)
                return 0;


            return (int)value;
        }

    }
    
}
