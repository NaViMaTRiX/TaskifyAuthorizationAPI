using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;

namespace AuthAPI.Shared.Helpers;

public static class IpAddressHelper
{
    public static string GetClientIpAddress(HttpContext context)
    {
        try
        {
            // 🔹 Проверяем `X-Forwarded-For` (список IP через запятую)
            var forwardedIps = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedIps))
            {
                // Берём первый "публичный" IP, если в списке несколько
                var ipList = forwardedIps.Split(',').Select(ip => ip.Trim()).ToList();
                var validIp = ipList.FirstOrDefault(IsValidIp);
                if (!string.IsNullOrEmpty(validIp)) return validIp;
            }

            // 🔹 Проверяем `X-Real-IP` (иногда используется вместо `X-Forwarded-For`)
            var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp) && IsValidIp(realIp))
            {
                return realIp;
            }

            // 🔹 Проверяем `RemoteIpAddress` (IP-адрес прямого подключения)
            var remoteIp = context.Connection.RemoteIpAddress?.ToString();
            if (!string.IsNullOrEmpty(remoteIp) && IsValidIp(remoteIp))
            {
                return remoteIp == "::1" || remoteIp == "127.0.0.1" ? "localhost" : remoteIp;
            }

            return "unknown"; // Если ничего не найдено
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при получении IP-адреса: {ex.Message}");
            return "unknown";
        }
    }

// 🔹 Метод для валидации IP-адреса
    private static bool IsValidIp(string ip)
    {
        return Regex.IsMatch(ip, @"^(?:\d{1,3}\.){3}\d{1,3}$|^(?:[a-fA-F0-9:]+:+)+[a-fA-F0-9]+$");
    }

}
