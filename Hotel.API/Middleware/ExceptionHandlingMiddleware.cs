using System.Text.Json;
using Hotel.Application.Common;

namespace Hotel.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex, _logger);
        }
    }

    public static async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception,
        ILogger logger)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, code) = exception switch
        {
            ValidationException => (StatusCodes.Status400BadRequest, "validation_error"),
            UnauthorizedException => (StatusCodes.Status401Unauthorized, "unauthorized"),
            NotFoundException => (StatusCodes.Status404NotFound, "not_found"),
            _ => (StatusCodes.Status500InternalServerError, "internal_error")
        };

        context.Response.StatusCode = statusCode;

        var response = new ApiErrorResponse
        {
            Code = code,
            Message = exception.Message,
            Status = statusCode,
            TraceId = context.TraceIdentifier,
            Timestamp = DateTimeOffset.UtcNow
        };

        if (statusCode >= 500)
        {
            logger.LogError(
                exception,
                "Unhandled exception on {Method} {Path}. TraceId={TraceId}",
                context.Request.Method,
                context.Request.Path,
                context.TraceIdentifier);
        }
        else
        {
            logger.LogWarning(
                "Handled exception {Code} on {Method} {Path}. Message={Message}. TraceId={TraceId}",
                code,
                context.Request.Method,
                context.Request.Path,
                exception.Message,
                context.TraceIdentifier);
        }

        var json = JsonSerializer.Serialize(response);
        await context.Response.WriteAsync(json);
    }
}
