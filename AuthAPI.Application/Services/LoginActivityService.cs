using System.Security.Cryptography;
using System.Text;
using AuthAPI.Application.Interface;
using AuthAPI.DAL.Interfaces;
using AuthAPI.Domain.Models;
using AuthAPI.Shared.Helpers;
using Microsoft.Extensions.Logging;

namespace AuthAPI.Application.Services;

public class LoginActivityService(
    ILoginActivitiesRepository activitiesRepository,
    ILogger<LoginActivityService> logger)
    : ILoginActivityService
{
    private const int MAX_FAILED_ATTEMPTS = 5;
    private const int BLOCK_DURATION_MINUTES = 15;

    /// <summary>
    /// Проверяет, разрешен ли вход пользователю, учитывая количество неудачных попыток
    /// и проверку на подозрительную активность
    /// </summary>
    public async Task<bool> IsLoginAllowedAsync(
        Guid userId, 
        string ipAddress, 
        string deviceFingerprint, 
        CancellationToken cancellationToken = default)
    {
        ValidateInputParameters(userId, ipAddress, deviceFingerprint);

        // Проверка количества неудачных попыток
        var failedAttempts = await GetFailedAttemptsCountAsync(userId, ipAddress, cancellationToken);
        if (failedAttempts >= MAX_FAILED_ATTEMPTS)
        {
            logger.LogWarning("Временная блокировка для пользователя {UserId}: превышено количество неудачных попыток входа", userId);
            return false;
        }

        // Проверка на подозрительную активность
        return !await IsSuspiciousLoginAsync(userId, ipAddress, deviceFingerprint, cancellationToken);
    }

    /// <summary>
    /// Логирует успешный вход в систему
    /// </summary>
    public async Task RecordSuccessfulLoginAsync(
        Guid userId, 
        string ipAddress, 
        string deviceInfo, 
        CancellationToken cancellationToken = default)
    {
        ValidateInputParameters(userId, ipAddress, deviceInfo);

        var deviceFingerprint = DeviceHelper.GenerateDeviceFingerprint(deviceInfo);
        var loginActivity = new UserLoginActivity
        {
            UserId = userId,
            IpAddress = ipAddress,
            DeviceType = ParseDeviceInfo(deviceInfo, DeviceInfoType.Type),
            DeviceBrowser = ParseDeviceInfo(deviceInfo, DeviceInfoType.Browser),
            DeviceOs = ParseDeviceInfo(deviceInfo, DeviceInfoType.OS),
            LoginTime = DateTime.UtcNow,
            LastActivityTime = DateTime.UtcNow,
            IsActive = true,
            IsSuspicious = false,
            FailedLoginAttempts = 0,
            DeviceFingerprint = deviceFingerprint
        };

        try
        {
            await activitiesRepository.CreateActivityAsync(loginActivity, cancellationToken);
            logger.LogInformation("Успешный вход для пользователя {UserId} с IP {IpAddress}", userId, ipAddress);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при записи успешного входа для пользователя {UserId}", userId);
            throw;
        }
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
        ValidateInputParameters(userId, ipAddress, deviceInfo);

        var deviceFingerprint = DeviceHelper.GenerateDeviceFingerprint(deviceInfo);
        var loginActivity = new UserLoginActivity
        {
            UserId = userId,
            IpAddress = ipAddress,
            DeviceType = ParseDeviceInfo(deviceInfo, DeviceInfoType.Type) ?? "Unknown",
            DeviceBrowser = ParseDeviceInfo(deviceInfo, DeviceInfoType.Browser),
            DeviceOs = ParseDeviceInfo(deviceInfo, DeviceInfoType.OS),
            LoginTime = DateTime.UtcNow,
            IsActive = false,
            IsSuspicious = true,
            FailedLoginAttempts = 1,
            DeviceFingerprint = deviceFingerprint
        };

        try
        {
            await activitiesRepository.CreateActivityAsync(loginActivity, cancellationToken);
            logger.LogWarning("Неудачный вход для пользователя {UserId} с IP {IpAddress}", userId, ipAddress);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при записи неудачного входа для пользователя {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Определяет подозрительность входа на основе нового устройства или новой локации
    /// </summary>
    public async Task<bool> IsSuspiciousLoginAsync(
        Guid userId, 
        string ipAddress, 
        string deviceFingerprint, 
        CancellationToken cancellationToken = default)
    {
        ValidateInputParameters(userId, ipAddress, deviceFingerprint);

        try
        {
            var recentLogins = await activitiesRepository.GetLastLoginsAsync(userId, cancellationToken);

            if (!recentLogins.Any())
            {
                logger.LogWarning("Подозрительный вход: пользователь {UserId} не имеет предыдущих логинов", userId);
                return true; // Первый вход считаем подозрительным
            }

            // Проверки на новое устройство и локацию
            var isNewDevice = recentLogins.All(x => x.DeviceFingerprint != deviceFingerprint);
            var isDifferentLocation = recentLogins.All(x => x.IpAddress != ipAddress);

            if (isNewDevice || isDifferentLocation)
            {
                logger.LogWarning(
                    "Подозрительный вход для пользователя {UserId}: новое устройство ({IsNewDevice}), новый IP ({IsDifferentLocation})",
                    userId, isNewDevice, isDifferentLocation);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при проверке подозрительного входа для пользователя {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Возвращает количество неудачных попыток входа для пользователя за определенный период
    /// </summary>
    private async Task<int> GetFailedAttemptsCountAsync(
        Guid userId, 
        string ipAddress, 
        CancellationToken cancellationToken)
    {
        try
        {
            var cutoffTime = DateTime.UtcNow.AddMinutes(-BLOCK_DURATION_MINUTES);
            return await activitiesRepository.GetFailedAttemptsCount(userId, ipAddress, cutoffTime, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при получении количества неудачных попыток входа для пользователя {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Проверяет и валидирует входные параметры
    /// </summary>
    private void ValidateInputParameters(Guid userId, string ipAddress, string additionalInfo = null!)
    {
        if (userId == Guid.Empty)
        {
            logger.LogError("Ошибка валидации: передан пустой `userId`");
            throw new ArgumentException("UserId не может быть пустым", nameof(userId));
        }

        if (string.IsNullOrWhiteSpace(ipAddress))
        {
            logger.LogError("Ошибка валидации: передан пустой `ipAddress`");
            throw new ArgumentException("IpAddress не может быть пустым", nameof(ipAddress));
        }

        if (additionalInfo != null && string.IsNullOrWhiteSpace(additionalInfo))
        {
            logger.LogError("Ошибка валидации: передан пустой `deviceInfo` или `deviceFingerprint`");
            throw new ArgumentException("Информация об устройстве не может быть пустой", nameof(additionalInfo));
        }
    }

    /// <summary>
    /// Типы информации об устройстве
    /// </summary>
    private enum DeviceInfoType
    {
        Type,
        Browser,
        OS
    }

    /// <summary>
    /// Извлекает нужную информацию из строки с данными об устройстве
    /// </summary>
    private string ParseDeviceInfo(string deviceInfo, DeviceInfoType infoType)
    {
        if (string.IsNullOrWhiteSpace(deviceInfo))
            return "Unknown";

        deviceInfo = deviceInfo.ToLower();
        
        return infoType switch
        {
            DeviceInfoType.Type => GetDeviceType(deviceInfo),
            DeviceInfoType.Browser => GetDeviceBrowser(deviceInfo),
            DeviceInfoType.OS => GetDeviceOs(deviceInfo),
            _ => "Unknown"
        };
    }

    private string GetDeviceType(string deviceInfo)
    {
        if (deviceInfo.Contains("mobile")) return "Mobile";
        if (deviceInfo.Contains("tablet")) return "Tablet";
        return "Desktop";
    }

    private string GetDeviceBrowser(string deviceInfo)
    {
        if (deviceInfo.Contains("chrome")) return "Chrome";
        if (deviceInfo.Contains("firefox")) return "Firefox";
        if (deviceInfo.Contains("safari")) return "Safari";
        if (deviceInfo.Contains("edge")) return "Edge";
        if (deviceInfo.Contains("yandex")) return "Yandex";
        return "Unknown";
    }

    private string GetDeviceOs(string deviceInfo)
    {
        if (deviceInfo.Contains("windows")) return "Windows";
        if (deviceInfo.Contains("mac")) return "MacOS";
        if (deviceInfo.Contains("linux")) return "Linux";
        if (deviceInfo.Contains("android")) return "Android";
        if (deviceInfo.Contains("ios")) return "iOS";
        return "Unknown";
    }
}