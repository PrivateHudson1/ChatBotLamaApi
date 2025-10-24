namespace ChatBotLamaApi.Interfaces
{
    public interface IRateLimiter
    {
        Task<string> CreateSessionAsync();
        Task<bool> ValidateSessionAsync(string userId);
        Task<bool> SessionExistsAsync(string userId);
        Task<bool> TryConsumeRequestAsync(string userId);
        Task<int> GetRemainingRequestsAsync(string userId);
    }
}
