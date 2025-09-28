using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;

namespace ChatBotLamaApi.Services
{
    public class ChatHub : Hub
    {
     
        private readonly ILogger<ChatHub> _logger;
        private readonly HttpClient _httpClient;
        private const string LlamaApiUrl = "http://192.168.0.3:8081/completion";

        public ChatHub(HttpClient httpClient, ILogger<ChatHub> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        [Authorize]
        public async Task SendMessage(string message)
        {
            var connectionId = Context.ConnectionId;
            try
            {
                await Clients.Caller.SendAsync("ReceiveMessage", "AI", "Thinking...");
                var prompt = $"<s>[INST] {message} [/INST]";

                var requestData = new
                {
                    prompt = prompt,
                    n_predict = 256,
                    temperature = 0.7,
                    top_p = 0.9,
                    repeat_penalty = 1.1,
                    stop = new[] { "</s>", "[INST]" },
                    stream = false
                };

                var json = JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(LlamaApiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    using var document = JsonDocument.Parse(responseJson);

                    if (document.RootElement.TryGetProperty("content", out var contentElement))
                    {
                        var aiResponse = contentElement.GetString()?.Trim();

                        if(!string.IsNullOrEmpty(aiResponse))
                        {
                            await Clients.Caller.SendAsync("ReceiveMessage", "AI", aiResponse);
                            _logger.LogInformation($"AI response sent to {connectionId}");
                        }
                        else
                        {
                            await Clients.Caller.SendAsync("ReceiveMessage", "AI", "Failed to generate the answer");
                        }
                    }
                        
                }
                else
                {
                    _logger.LogError($"Llama APi error: {response.StatusCode}");
                    await Clients.Caller.SendAsync("ReceiveMessage", "AI", "Network connection error");
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing from {connectionId}");
                await Clients.Caller.SendAsync("ReceiveMessage", "AI", "There was an error when processing a request");
            }
          
        }


        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation($"Client connected: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation($"Client disconnected: {Context.ConnectionId}");
            await base.OnDisconnectedAsync(exception);
        }
        

    }
}
