using ChatBotLamaApi.Interfaces;
using StackExchange.Redis;

namespace ChatBotLamaApi.Services
{
    public class RedisRateLimiter : IRateLimiter
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly TimeSpan _sessionLifetime = TimeSpan.FromDays(1);

        public RedisRateLimiter(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        public async Task<string> CreateSessionAsync()
        {
            var db = _redis.GetDatabase();
            var newUserId = Guid.NewGuid().ToString();
            var sessionKey = $"session:{newUserId}";

            await db.StringSetAsync(sessionKey, "active", _sessionLifetime);
            return newUserId;
        }

        public async Task<bool> ValidateSessionAsync(string userId)
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

        public async Task<bool> SessionExistsAsync(string userId)
        {
            var db = _redis.GetDatabase();
            var sessionKey = $"session:{userId}";
            return await db.KeyExistsAsync(sessionKey);
        }
    }
    
}
