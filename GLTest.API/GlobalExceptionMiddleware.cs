using System.Text.Json;
using System.Text.RegularExpressions;
using GLTest.Application.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
 

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ProblemException ex)  
        {
            _logger.LogWarning("ProblemException: {Error}", ex.Error);

            var response = Result<object>.FailureResult(ex.Error, new List<string> { ex.Message });

            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
        catch (SqlException ex)   
        {
            await HandleSqlException(context, ex);  
        }
        catch (Exception ex)  
        {
            _logger.LogError(ex, "Unhandled exception occurred.");

            var problemDetails = new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred.",
                Status = StatusCodes.Status500InternalServerError,
                Instance = context.Request.Path
            };

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
        }
    }

    private string ExtractSqlErrorMessage(SqlException ex)
    {
        foreach (SqlError error in ex.Errors)
        {
            if (error.Number == 2601 || error.Number == 2627)  
            {
                var fieldName = ExtractFieldName(error.Message);
                return fieldName != null
                    ? $"A record with the same value for '{fieldName}' already exists."
                    : "A record with duplicate data already exists.";
            }
            if (error.Number == 547)  
            {
                return "A foreign key constraint prevents this action.";
            }
        }
        return "A database error occurred.";
    }

    private string? ExtractFieldName(string errorMessage)
    {      
        var match = Regex.Match(errorMessage, @"\bconstraint\s+'?(?<constraintName>\w+)'?", RegexOptions.IgnoreCase);
        if (match.Success)
        {
            string constraintName = match.Groups["constraintName"].Value;
            return FormatConstraintName(constraintName);
        }

        return null;
    }

    private string FormatConstraintName(string constraintName)
    {      
        if (constraintName.Contains("_"))
        {
            var parts = constraintName.Split('_');
            if (parts.Length >= 3) 
            {
                return parts[^1]; 
            }
        }
        return constraintName;
    }

    private async Task HandleSqlException(HttpContext context, SqlException ex)
    {
        _logger.LogError(ex, "SQL Exception occurred.");

        var sqlErrorMessage = ExtractSqlErrorMessage(ex);

        var errorsDict = new Dictionary<string, string[]>
    {
        { "Database", new[] { sqlErrorMessage } }   
    };

        var response = new
        {
            success = false,
            message = "Database Error",
            data = (object)null,
            errors = errorsDict,
            statusCode = 500
        };

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }

    public class ProblemException : Exception
    {
        public string Error { get; }
        public object Errors { get; }

        public ProblemException(string error, object errors) : base(error)
        {
            Error = error;
            Errors = errors;
        }
    }
}
