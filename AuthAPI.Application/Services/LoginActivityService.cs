using System.Security.Cryptography;
using System.Text;
using AuthAPI.Application.Interface;
using AuthAPI.DAL.Data;
using AuthAPI.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace AuthAPI.Application.Services;

public class LoginActivityService(
    AuthDbContext context, 
    IConfiguration configuration) : ILoginActivityService
{
    private const int MAX_FAILED_ATTEMPTS = 5;
    private const int BLOCK_DURATION_MINUTES = 15;

    /// <summary>
    /// Реализует логику определения подозрительного входа в систему
    /// </summary>
    public async Task<bool> IsLoginAllowedAsync(
        Guid userId, 
        string ipAddress, 
        string deviceFingerprint, 
        CancellationToken cancellationToken = default)
    {
        // Проверка количества неудачных попыток
        var failedAttempts = await GetFailedAttemptsCount(userId, ipAddress, cancellationToken);
        if (failedAttempts >= MAX_FAILED_ATTEMPTS)
        {
            // Временная блокировка
            return false;
        }

        // Проверка на подозрительную активность
        return !await IsSuspiciousLoginAsync(userId, ipAddress, deviceFingerprint, cancellationToken);
    }

    /// <summary>
    /// Логирует удачный вход в систему
    /// </summary>
    public async Task RecordSuccessfulLoginAsync(
        Guid userId, 
        string ipAddress, 
        string deviceInfo, 
        CancellationToken cancellationToken = default)
    {
        var loginActivity = new UserLoginActivity
        {
            UserId = userId,
            IpAddress = ipAddress,
            DeviceType = GetDeviceType(deviceInfo),
            DeviceBrowser = GetDeviceBrowser(deviceInfo),
            DeviceOs = GetDeviceOs(deviceInfo),
            LoginTime = DateTime.UtcNow,
            LastActivityTime = DateTime.UtcNow,
            IsActive = true,
            IsSuspicious = false,
            FailedLoginAttempts = 0,
            DeviceFingerprint = GenerateDeviceFingerprint(deviceInfo)
        };

        context.UserLoginActivities.Add(loginActivity);
        await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Логирует неудачный вход в систему
    /// </summary>
    public async Task RecordFailedLoginAsync(
        Guid userId, 
        string ipAddress, 
        string deviceInfo, 
        CancellationToken cancellationToken = default)
    {
        var loginActivity = new UserLoginActivity
        {
            UserId = userId,
            IpAddress = ipAddress,
            DeviceType = GetDeviceType(deviceInfo),
            DeviceBrowser = GetDeviceBrowser(deviceInfo),
            DeviceOs = GetDeviceOs(deviceInfo),
            LoginTime = DateTime.UtcNow,
            IsActive = false,
            IsSuspicious = true,
            FailedLoginAttempts = 1,
            DeviceFingerprint = GenerateDeviceFingerprint(deviceInfo)
        };

        context.UserLoginActivities.Add(loginActivity);
        await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Определения нового device и новой локации
    /// </summary>
    public async Task<bool> IsSuspiciousLoginAsync(
        Guid userId, 
        string ipAddress, 
        string deviceFingerprint, 
        CancellationToken cancellationToken = default)
    {
        var recentLogins = await context.UserLoginActivities
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.LoginTime)
            .Take(5)
            .ToListAsync(cancellationToken);

        // Проверка на новые устройства
        var isNewDevice = recentLogins.All(x => x.DeviceFingerprint != deviceFingerprint);

        // Проверка на входы с разных IP
        var isDifferentLocation = recentLogins.Any(x => x.IpAddress != ipAddress);

        return isNewDevice || isDifferentLocation;
    }

    /// <summary>
    /// Возвращает количество неудачных попыток входа для пользователя
    /// </summary>
    private async Task<int> GetFailedAttemptsCount(
        Guid userId, 
        string ipAddress, 
        CancellationToken cancellationToken)
    {
        return await context.UserLoginActivities
            .Where(x => x.UserId == userId && 
                        x.IpAddress == ipAddress && 
                        x.LoginTime > DateTime.UtcNow.AddMinutes(-BLOCK_DURATION_MINUTES))
            .CountAsync(cancellationToken);
    }

    private string GenerateDeviceFingerprint(string deviceInfo)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(deviceInfo));
        return Convert.ToBase64String(hashBytes);
    }

    private string GetDeviceType(string deviceInfo)
    {
        // Простая логика определения типа устройства
        deviceInfo = deviceInfo.ToLower();
        if (deviceInfo.Contains("mobile")) return "Mobile";
        if (deviceInfo.Contains("tablet")) return "Tablet";
        return "Desktop";
    }

    private string GetDeviceBrowser(string deviceInfo)
    {
        // Простая логика определения браузера
        deviceInfo = deviceInfo.ToLower();
        if (deviceInfo.Contains("chrome")) return "Chrome";
        if (deviceInfo.Contains("firefox")) return "Firefox";
        if (deviceInfo.Contains("safari")) return "Safari";
        if (deviceInfo.Contains("edge")) return "Edge";
        if (deviceInfo.Contains("yandex")) return "Yandex";
        return "Unknown";
    }

    private string GetDeviceOs(string deviceInfo)
    {
        // Простая логика определения операционной системы
        deviceInfo = deviceInfo.ToLower();
        if (deviceInfo.Contains("windows")) return "Windows";
        if (deviceInfo.Contains("mac")) return "MacOS";
        if (deviceInfo.Contains("linux")) return "Linux";
        if (deviceInfo.Contains("android")) return "Android";
        if (deviceInfo.Contains("ios")) return "iOS";
        return "Unknown";
    }
}
