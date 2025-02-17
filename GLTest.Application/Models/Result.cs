using System.Text.Json.Serialization;

namespace GLTest.Application.Common
{
    public class Result<T>
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("data")]
        public T? Data { get; set; }

        [JsonPropertyName("errors")]
        public Dictionary<string, string[]> Errors { get; set; } = new();

        [JsonPropertyName("statusCode")]
        public int? StatusCode { get; set; }

        private Result(bool success, string message, T? data = default, Dictionary<string, string[]>? errors = null, int? statusCode = null)
        {
            Success = success;
            Message = message;
            Data = data;
            Errors = errors ?? new Dictionary<string, string[]>();
            StatusCode = statusCode;
        }

        public static Result<T> SuccessResult(T data, string message = "Request successful")
            => new(true, message, data);

        public static Result<T> FailureResult(string message, List<string>? errorMessages = null)
            => new(false, message, default, errorMessages?.ToDictionary(k => k, v => new string[] { v }));

        public static Result<T> FailureResult(string message, Dictionary<string, string[]> errors, int statusCode = 400)
            => new(false, message, default, errors, statusCode);
          
        public static Result<T> Problem(string title, string detail, int statusCode)
            => new(false, title, default, new Dictionary<string, string[]> { { "Error", new string[] { detail } } }, statusCode);
    }
}
