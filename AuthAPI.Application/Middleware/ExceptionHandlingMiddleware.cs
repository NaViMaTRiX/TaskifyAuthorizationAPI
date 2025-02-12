using System.Net;
using AuthAPI.Shared.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using UnauthorizedAccessException = System.UnauthorizedAccessException;

namespace AuthAPI.Application.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        logger.LogError(exception, "Ошибка в Authorization API: {Message}", exception.Message);

        var statusCode = exception switch
        {
            InvalidCredentialsException => HttpStatusCode.Unauthorized,   // 401
            SecurityTokenException => HttpStatusCode.Unauthorized,        // 401
            UnauthorizedAccessException => HttpStatusCode.Forbidden,      // 403
            UserAlreadyExistsException => HttpStatusCode.Conflict,        // 409
            TooManyRequestsException => HttpStatusCode.TooManyRequests,   // 429
            DatabaseConnectionException => HttpStatusCode.InternalServerError, // 500
            _ => HttpStatusCode.InternalServerError                      // По умолчанию 500
        };

        var response = new
        {
            error = exception.Message,
            statusCode = (int)statusCode,
            timespan = DateTime.UtcNow + DateTime.UtcNow.Subtract(DateTime.Now) //TODO: Это надо проверить на работаспособность
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        await context.Response.WriteAsJsonAsync(response);
    }
}