namespace ChatBotLamaApi.Interfaces
{
    public interface IRateLimiter
    {
        Task<bool> AllowRequestAsync(string userId);
    }
}
