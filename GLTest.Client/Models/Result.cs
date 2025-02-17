using Newtonsoft.Json;


namespace GLTest.Client.Models
{

    public class Result<T>
    {
   
        public bool Success { get; set; }

      
        public string Message { get; set; }


        public T? Data { get; set; }

      
        public Dictionary<string, string[]> Errors { get; set; } = new();

     
        public int? StatusCode { get; set; }

        [JsonConstructor]
        public Result() { }

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

