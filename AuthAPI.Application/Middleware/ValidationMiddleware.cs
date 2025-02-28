using Microsoft.AspNetCore.Http;

namespace AuthAPI.Application.Middleware;

public class ValidationMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext context)
    {
        var originalBodyStream = context.Response.Body;
        using var memoryStream = new MemoryStream();
        context.Response.Body = memoryStream;

        await next(context);

        if (context.Response.StatusCode == 400 && context.Items.ContainsKey("errors"))
        {
            var errors = context.Items["errors"];
            context.Response.Body = originalBodyStream;
            await context.Response.WriteAsJsonAsync(new { Errors = errors });
        }
        else
        {
            memoryStream.Seek(0, SeekOrigin.Begin);
            await memoryStream.CopyToAsync(originalBodyStream);
        }
    }
}

