using Microsoft.AspNetCore.Http;

namespace AuthAPI.Shared.Helpers;

public static class IpAddressHelper
{
    public static string? GetClientIpAddress(HttpContext context)
    {
        // Проверяем заголовки прокси-серверов в первую очередь
        string? ipAddress = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        
        if (string.IsNullOrEmpty(ipAddress))
        {
            // Если нет заголовков прокси, используем RemoteIpAddress
            ipAddress = context.Connection.RemoteIpAddress?.ToString();
        }

        // Обработка особых случаев
        if (string.IsNullOrEmpty(ipAddress) || ipAddress == "::1")
        {
            ipAddress = "localhost"; // localhost
        }

        return ipAddress;
    }
}
