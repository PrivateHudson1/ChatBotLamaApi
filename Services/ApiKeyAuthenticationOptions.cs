using Microsoft.AspNetCore.Authentication;

namespace ChatBotLamaApi.Services
{
    public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
    {
        public const string DefaultScheme = "ApiKey";
        public string ApiKey { get; set; } = string.Empty;
    }
}
