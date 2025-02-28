using System.Security.Cryptography;
using System.Text;

namespace AuthAPI.Shared.Helpers;

public class DeviceHelper
{
    /// <summary>
    /// Генерирует уникальный отпечаток устройства
    /// </summary>
    public static string GenerateDeviceFingerprint(string deviceInfo)
    {
        var hashBytes = SHA3_512.HashData(Encoding.UTF8.GetBytes(deviceInfo));
        return Convert.ToBase64String(hashBytes);
    }
}