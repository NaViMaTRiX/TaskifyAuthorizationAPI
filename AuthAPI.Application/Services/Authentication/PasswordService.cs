using System.Security.Cryptography;
using AuthAPI.Application.Interface;

namespace AuthAPI.Application.Services.Authentication;

/// <summary>
/// Сервис для работы с паролями
/// </summary>
public class PasswordService : IPasswordService
{
    private const int SaltSize = 16; // 128 bit 
    private const int KeySize = 32; // 256 bit
    private const int Iterations = 10000;
    private static readonly HashAlgorithmName HashAlgorithm = HashAlgorithmName.SHA256;

    /// <summary>
    /// Хеширование пароля с использованием соли и PBKDF2
    /// </summary>
    /// <param name="password">Исходный пароль</param>
    /// <returns>Захешированный пароль с солью</returns>
    public string HashPassword(string password)
    {
        // Генерация криптографически безопасной соли
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        
        // Хеширование пароля с использованием PBKDF2
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithm,
            KeySize
        );

        // Объединение соли и хеша для хранения
        return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }

    /// <summary>
    /// Проверка пароля
    /// </summary>
    /// <param name="password">Пароль для проверки</param>
    /// <param name="hashedPasswordWithSalt">Захешированный пароль с солью</param>
    /// <returns>Результат проверки пароля</returns>
    public bool VerifyPassword(string password, string hashedPasswordWithSalt)
    {
        // Разделение соли и хеша
        var parts = hashedPasswordWithSalt.Split(':');
        var salt = Convert.FromBase64String(parts[0]);
        var storedHash = Convert.FromBase64String(parts[1]);

        // Хеширование входного пароля с той же солью
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithm,
            KeySize
        );

        // Сравнение хешей
        return CryptographicOperations.FixedTimeEquals(hash, storedHash);
    }

    /// <summary>
    /// Генерация случайного пароля
    /// </summary>
    /// <param name="length">Длина пароля</param>
    /// <returns>Сгенерированный пароль</returns>
    public string GenerateRandomPassword(int length = 12)
    {
        const string validChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()_+-=[]{}|;:,.<>?";
        
        var randomBytes = new byte[length];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }

        return new string(randomBytes.Select(x => validChars[x % validChars.Length]).ToArray());
    }

    /// <summary>
    /// Проверка сложности пароля
    /// </summary>
    /// <param name="password">Пароль для проверки</param>
    /// <returns>Результат проверки сложности</returns>
    public bool IsPasswordComplex(string password)
    {
        return password.Length >= 8 &&
               password.Any(char.IsUpper) &&
               password.Any(char.IsLower) &&
               password.Any(char.IsDigit) &&
               password.Any(ch => !char.IsLetterOrDigit(ch));
    }
}
