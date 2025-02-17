
using GLTest.Client.Models;
using System.Text;
using System.Text.Json;

namespace GLTest.Client.Services
{
    public class TokenService : ITokenService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private const string TokenSessionKey = "JwtToken";
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        public TokenService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }

        public async Task<string> GetTokenAsync()
        {
            return TryGetTokenFromSession() ?? await FetchNewTokenAsync();
        }

        private string? TryGetTokenFromSession()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            return session?.GetString(TokenSessionKey);
        }

        public async Task<string> FetchNewTokenAsync()
        {
            var authEndpoint = _configuration["ApiSettings:AuthUrl"];
            var clientId = _configuration["ApiSettings:ClientId"];
            var clientSecret = _configuration["ApiSettings:ClientSecret"];

            var requestBody = new { username = clientId, password = clientSecret };
            var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            using var client = _httpClientFactory.CreateClient("AuthClient");
            var response = await client.PostAsync(authEndpoint, jsonContent);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Failed to obtain JWT token. Status Code: {response.StatusCode}");

            var responseBody = await response.Content.ReadAsStringAsync();
            var tokenResponse = TryDeserialize<TokenResponse>(responseBody);

            if (string.IsNullOrEmpty(tokenResponse?.Token))
                throw new Exception("Invalid token received.");

            SaveTokenToSession(tokenResponse.Token);
            return tokenResponse.Token;
        }

        private void SaveTokenToSession(string token)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            session?.SetString(TokenSessionKey, token);
        }

        private T? TryDeserialize<T>(string json)
        {
            try
            {
                return JsonSerializer.Deserialize<T>(json, _jsonOptions);
            }
            catch (JsonException)
            {
                return default;
            }
        }
    }
}



