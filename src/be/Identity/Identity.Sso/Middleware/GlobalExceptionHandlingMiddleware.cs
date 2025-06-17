using Identity.Contracts.Common;
using System.Net;
using System.Text.Json;

namespace Identity.Sso.Middleware;

public class GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";        var response = new ApiResponse<object>
        {
            Success = false
        };

        switch (exception)
        {
            case ArgumentException argumentException:
                response = response with { Message = argumentException.Message };
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                break;

            case InvalidOperationException invalidOperationException:
                response = response with { Message = invalidOperationException.Message };
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                break;

            case UnauthorizedAccessException:
                response = response with { Message = "Unauthorized access" };
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                break;

            case KeyNotFoundException:
                response = response with { Message = "Resource not found" };
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                break;

            default:
                response = response with { Message = "An internal server error occurred" };
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                break;
        }

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}
