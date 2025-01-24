using System.Net;
using System.Text.Json;

namespace ClinicalTrialApp.Common.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during request processing");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var response = new
        {
            Status = "Error",
            Message = GetUserFriendlyMessage(exception)
        };

        context.Response.StatusCode = GetStatusCode(exception);

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }

    private static string GetUserFriendlyMessage(Exception exception)
    {
        return exception switch
        {
            BadHttpRequestException => "Invalid request format or missing required data",
            FileNotFoundException => "The requested file was not found",
            UnauthorizedAccessException => "You don't have permission to perform this action",
            // Add more specific exceptions as needed
            _ => "An unexpected error occurred. Please try again later"
        };
    }

    private static int GetStatusCode(Exception exception)
    {
        return exception switch
        {
            BadHttpRequestException => (int)HttpStatusCode.BadRequest,
            FileNotFoundException => (int)HttpStatusCode.NotFound,
            UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
            _ => (int)HttpStatusCode.InternalServerError
        };
    }
} 