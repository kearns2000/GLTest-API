using GLTest.Client.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace GLTest.Client.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;

        public CompanyService(IHttpClientFactory httpClientFactory, ITokenService tokenService, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _tokenService = tokenService;
            _configuration = configuration;
        }

        private async Task<HttpClient> CreateAuthorizedClientAsync()
        {
            var token = await _tokenService.GetTokenAsync();
            var client = _httpClientFactory.CreateClient("Company");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        public async Task<Result<List<Company>>> GetCompaniesAsync()
        {
            var url = $"{_configuration["ApiSettings:BaseUrl"]}{_configuration["ApiSettings:CompanyApiUrl"]}";
            return await SendRequestAsync<List<Company>>(HttpMethod.Get, url);
        }

        public async Task<Result<Company>> GetCompanyAsync(Guid id)
        {
            var url = $"{_configuration["ApiSettings:BaseUrl"]}{_configuration["ApiSettings:CompanyApiUrl"]}/{id}";
            return await SendRequestAsync<Company>(HttpMethod.Get, url);
        }

        public async Task<Result<Company>> GetCompanyByIsinAsync(string isin)
        {
            var url = $"{_configuration["ApiSettings:BaseUrl"]}{_configuration["ApiSettings:CompanyApiUrl"]}/isin/{isin}";
            return await SendRequestAsync<Company>(HttpMethod.Get, url);
        }

        public async Task<Result<bool>> CreateCompanyAsync(Company model)
        {
            var url = $"{_configuration["ApiSettings:BaseUrl"]}{_configuration["ApiSettings:CompanyApiUrl"]}";
            return await SendRequestAsync<bool>(HttpMethod.Post, url, model);
        }

        public async Task<Result<bool>> UpdateCompanyAsync(Company model)
        {
            var url = $"{_configuration["ApiSettings:BaseUrl"]}{_configuration["ApiSettings:CompanyApiUrl"]}/{model.Id}";
            return await SendRequestAsync<bool>(HttpMethod.Put, url, model);
        }

        private async Task<Result<T>> SendRequestAsync<T>(HttpMethod method, string url, object? payload = null)
        {
            using var client = await CreateAuthorizedClientAsync();
            using var request = new HttpRequestMessage(method, url);

            if (payload != null)
            {
                var jsonContent = JsonConvert.SerializeObject(payload);
                request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            }

            var response = await client.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"Raw JSON Response: {responseContent}");

            if (!response.IsSuccessStatusCode)
            {
                var problemDetails = TryDeserialize<ProblemDetails>(responseContent);
                if (problemDetails != null)
                {
                    var errors = problemDetails.Extensions.TryGetValue("errors", out var errorsJson)
                        ? JsonConvert.DeserializeObject<Dictionary<string, string[]>>(errorsJson?.ToString() ?? "{}")
                        : new Dictionary<string, string[]>();

                    return Result<T>.FailureResult(problemDetails.Detail ?? "Request failed", errors);
                }

                return Result<T>.FailureResult("Unexpected error occurred.");
            }

            var result = TryDeserialize<Result<T>>(responseContent);
            if (result == null || result.Data == null)
            {             
                return Result<T>.FailureResult("Invalid response from server.");
            }

            return result;
        }

        private T? TryDeserialize<T>(string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"JSON deserialization error: {ex.Message}");
                return default;
            }
        }
    }
}




