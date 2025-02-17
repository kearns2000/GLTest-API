using System.Text.Json.Serialization;

namespace GLTest.Client.Models
{
    public class TokenResponse
    {
        [JsonPropertyName("token")]
        public string Token { get; set; }
    }
}
