using ChatBotLamaApi.Interfaces;
using StackExchange.Redis;

namespace ChatBotLamaApi.Services
{
    public class RedisRateLimiter : IRateLimiter
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly int _maxRequestsPerDay = 30;

        public RedisRateLimiter(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        public async Task<bool> AllowRequestAsync(string userId)
        {
            var db = _redis.GetDatabase();

            var key = $"requests:{userId}:{DateTime.UtcNow:yyyyMMdd}";

         
            var count = await db.StringIncrementAsync(key);

    
            if (count == 1)
            {
                await db.KeyExpireAsync(key, TimeSpan.FromDays(1));
            }

            return count <= _maxRequestsPerDay;
        }
    }
}
