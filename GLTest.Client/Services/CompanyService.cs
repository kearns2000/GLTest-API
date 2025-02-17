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






//using GLTest.Client.Models;
//using Microsoft.AspNetCore.Mvc;
//using System.Net.Http.Headers;
//using System.Text;
//using System.Text.Json;

//namespace GLTest.Client.Services
//{
//    public class CompanyService : ICompanyService
//    {
//        private readonly IHttpClientFactory _httpClientFactory;
//        private readonly ITokenService _tokenService;
//        private readonly IConfiguration _configuration;
//        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
//        {
//            PropertyNameCaseInsensitive = true
//        };

//        public CompanyService(IHttpClientFactory httpClientFactory, ITokenService tokenService, IConfiguration configuration)
//        {
//            _httpClientFactory = httpClientFactory;
//            _tokenService = tokenService;
//            _configuration = configuration;
//        }

//        private async Task<HttpClient> CreateAuthorizedClientAsync()
//        {
//            var token = await _tokenService.GetTokenAsync();
//            var client = _httpClientFactory.CreateClient("Company");
//            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
//            return client;
//        }


//        public async Task<Result<List<Company>>> GetCompaniesAsync()
//        {
//            var url = $"{_configuration["ApiSettings:BaseUrl"]}{_configuration["ApiSettings:CompanyApiUrl"]}";
//            return await SendRequestAsync<List<Company>>(HttpMethod.Get, url);
//        }

//        public async Task<Result<Company>> GetCompanyAsync(Guid id)
//        {
//            var url = $"{_configuration["ApiSettings:BaseUrl"]}{_configuration["ApiSettings:CompanyApiUrl"]}/{id}";
//            return await SendRequestAsync<Company>(HttpMethod.Get, url);
//        }

//        public async Task<Result<Company>> GetCompanyByIsinAsync(string isin)
//        {
//            var url = $"{_configuration["ApiSettings:BaseUrl"]}{_configuration["ApiSettings:CompanyApiUrl"]}/isin/{isin}";
//            return await SendRequestAsync<Company>(HttpMethod.Get, url);
//        }

//        public async Task<Result<bool>> CreateCompanyAsync(Company model)
//        {
//            var url = $"{_configuration["ApiSettings:BaseUrl"]}{_configuration["ApiSettings:CompanyApiUrl"]}";
//            return await SendRequestAsync<bool>(HttpMethod.Post, url, model);
//        }

//        public async Task<Result<bool>> UpdateCompanyAsync(Company model)
//        {
//            var url = $"{_configuration["ApiSettings:BaseUrl"]}{_configuration["ApiSettings:CompanyApiUrl"]}/{model.Id}";
//            return await SendRequestAsync<bool>(HttpMethod.Put, url, model);
//        }

//        private async Task<Result<T>> SendRequestAsync<T>(HttpMethod method, string url, object? payload = null)
//        {
//            using var client = await CreateAuthorizedClientAsync();
//            using var request = new HttpRequestMessage(method, url);

//            if (payload != null)
//            {
//                var jsonContent = JsonSerializer.Serialize(payload);
//                request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
//            }

//            var response = await client.SendAsync(request);
//            var responseContent = await response.Content.ReadAsStringAsync();

//            if (!response.IsSuccessStatusCode)
//            {
//                var problemDetails = TryDeserialize<ProblemDetails>(responseContent);
//                if (problemDetails != null)
//                {
//                    var errors = problemDetails.Extensions.TryGetValue("errors", out var errorsJson)
//                        ? TryDeserialize<Dictionary<string, string[]>>(errorsJson?.ToString())
//                        : new Dictionary<string, string[]>();

//                    return Result<T>.FailureResult(problemDetails.Detail ?? "Request failed", errors);
//                }

//                return Result<T>.FailureResult("Unexpected error occurred.");
//            }

//            var result = TryDeserialize<Result<T>>(responseContent);
//            return result ?? Result<T>.FailureResult("Invalid response from server.");
//        }

//        private T? TryDeserialize<T>(string json)
//        {
//            try
//            {
//                return JsonSerializer.Deserialize<T>(json, _jsonOptions);
//            }
//            catch (JsonException)
//            {
//                return default;
//            }
//        }

//    }
//}



//using GLTest.Client.Models;
//using Microsoft.AspNetCore.Mvc;
//using System.Net.Http.Headers;
//using System.Text;
//using System.Text.Json;


//namespace GLTest.Client.Services
//{
//    public class CompanyService : ICompanyService
//    {
//        private readonly IHttpClientFactory _httpClientFactory;
//        private readonly ITokenService _tokenService;
//        private readonly IConfiguration _configuration;

//      private readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions
//      {
//          PropertyNameCaseInsensitive = true       
//      };

//        public CompanyService(IHttpClientFactory httpClientFactory,
//                              ITokenService tokenService,
//                              IConfiguration configuration)
//        {
//            _httpClientFactory = httpClientFactory;
//            _tokenService = tokenService;
//            _configuration = configuration;
//        }


//        private async Task<HttpClient> CreateAuthorizedClientAsync()
//        {
//            var token = await _tokenService.GetTokenAsync();
//            var client = _httpClientFactory.CreateClient("Company");
//            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
//            return client;
//        }

//        public async Task<Result<List<Company>>> GetCompaniesAsync()
//        {
//            var apiUrl = $"{_configuration["ApiSettings:BaseUrl"]}{_configuration["ApiSettings:CompanyApiUrl"]}";

//            using var client = await CreateAuthorizedClientAsync();
//            var response = await client.GetAsync(apiUrl);

//            if (!response.IsSuccessStatusCode)
//                return Result<List<Company>>.FailureResult("Failed to retrieve companies.");

//            var content = await response.Content.ReadAsStringAsync();
//            var result = JsonSerializer.Deserialize<Result<List<Company>>>(content,jsonOptions);

//            return result ?? Result<List<Company>>.FailureResult("Invalid response from server.");
//        }

//        public async Task<Result<Company>> GetCompanyAsync(Guid id)
//        {
//            var apiUrl = $"{_configuration["ApiSettings:BaseUrl"]}{_configuration["ApiSettings:CompanyApiUrl"]}/{id}";

//            using var client = await CreateAuthorizedClientAsync();
//            var response = await client.GetAsync(apiUrl);

//            if (!response.IsSuccessStatusCode)
//                return Result<Company>.FailureResult("Company not found.");


//            var content = await response.Content.ReadAsStringAsync();
//            var result = JsonSerializer.Deserialize<Result<Company>>(content, jsonOptions);

//            return result ?? Result<Company>.FailureResult("Invalid response from server.");
//        }

//        public async Task<Result<Company>> GetCompanyByIsinAsync(string isin)
//        {
//            var apiUrl = $"{_configuration["ApiSettings:BaseUrl"]}{_configuration["ApiSettings:CompanyApiUrl"]}/isin/{isin}";

//            using var client = await CreateAuthorizedClientAsync();
//            var response = await client.GetAsync(apiUrl);

//            if (!response.IsSuccessStatusCode)
//                return Result<Company>.FailureResult("Company not found.");

//            var content = await response.Content.ReadAsStringAsync();
//            var result = JsonSerializer.Deserialize<Result<Company>>(content, jsonOptions);

//            return result ?? Result<Company>.FailureResult("Invalid response from server.");
//        }

//        public async Task<Result<bool>> CreateCompanyAsync(Company model)
//        {
//            var apiUrl = $"{_configuration["ApiSettings:BaseUrl"]}{_configuration["ApiSettings:CompanyApiUrl"]}";
//            var jsonContent = JsonSerializer.Serialize(model);
//            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

//            using var client = await CreateAuthorizedClientAsync();
//            var response = await client.PostAsync(apiUrl, content);
//            var responseContent = await response.Content.ReadAsStringAsync();

//            if (!response.IsSuccessStatusCode)
//            {
//                try
//                {               
//                    var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseContent);
//                    if (problemDetails != null)
//                    {
//                        var errors = problemDetails.Extensions.ContainsKey("errors")
//                            ? JsonSerializer.Deserialize<Dictionary<string, string[]>>(problemDetails.Extensions["errors"].ToString())
//                            : new Dictionary<string, string[]>();

//                        return Result<bool>.FailureResult(problemDetails.Detail ?? "Failed to create company.", errors);
//                    }
//                }
//                catch (JsonException)
//                {               
//                    return Result<bool>.FailureResult("Unexpected error occurred.", new Dictionary<string, string[]>());
//                }
//            }

//            var result = JsonSerializer.Deserialize<Result<bool>>(responseContent);
//            if (result == null || !result.Success)
//                return Result<bool>.FailureResult(result?.Message ?? "Failed to create company.", result?.Errors);

//            return Result<bool>.SuccessResult(true, "Company created successfully.");
//        }


//        public async Task<Result<bool>> UpdateCompanyAsync(Company model)
//        {
//            var apiUrl = $"{_configuration["ApiSettings:BaseUrl"]}{_configuration["ApiSettings:CompanyApiUrl"]}/{model.Id}";
//            var jsonContent = JsonSerializer.Serialize(model);
//            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

//            using var client = await CreateAuthorizedClientAsync();
//            var response = await client.PutAsync(apiUrl, content);
//            var responseContent = await response.Content.ReadAsStringAsync();

//            var result = JsonSerializer.Deserialize<Result<object>>(responseContent);

//            if (!response.IsSuccessStatusCode || result == null || !result.Success)
//                return Result<bool>.FailureResult(result?.Message ?? "Failed to update company.", result?.Errors);

//            return Result<bool>.SuccessResult(true, "Company updated successfully.");
//        }   

//    }
//}

