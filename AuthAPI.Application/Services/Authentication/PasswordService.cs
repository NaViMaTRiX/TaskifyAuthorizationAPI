using System.Security.Cryptography;
using System.Text.RegularExpressions;
using AuthAPI.Application.Interface;

namespace AuthAPI.Application.Services.Authentication;

/// <summary>
/// Сервис для работы с паролями
/// </summary>
public class PasswordService : IPasswordService
{
    private const int SaltSize = 16; // 128 бит
    private const int KeySize = 32; // 256 бит
    private const int Iterations = 10000;
    private static readonly HashAlgorithmName HashAlgorithm = HashAlgorithmName.SHA256;

    private static readonly Regex PasswordComplexityRegex = new(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[\W_]).{8,}$", RegexOptions.Compiled);

    /// <summary>
    /// Хеширование пароля с использованием соли и PBKDF2
    /// </summary>
    /// <param name="password">Исходный пароль</param>
    /// <returns>Захешированный пароль с солью</returns>
    public string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Пароль не может быть пустым.", nameof(password));

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithm, KeySize);

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
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Пароль не может быть пустым.", nameof(password));

        if (string.IsNullOrWhiteSpace(hashedPasswordWithSalt) || !hashedPasswordWithSalt.Contains(":"))
            throw new ArgumentException("Неверный формат хешированного пароля.", nameof(hashedPasswordWithSalt));

        var parts = hashedPasswordWithSalt.Split(':');
        if (parts.Length != 2)
            throw new FormatException("Неверный формат хешированного пароля.");

        var salt = Convert.FromBase64String(parts[0]);
        var storedHash = Convert.FromBase64String(parts[1]);

        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithm, KeySize);

        return CryptographicOperations.FixedTimeEquals(hash, storedHash);
    }

    /// <summary>
    /// Генерация случайного пароля
    /// </summary>
    /// <param name="length">Длина пароля</param>
    /// <returns>Сгенерированный пароль</returns>
    public string GenerateRandomPassword(int length = 12)
    {
        if (length < 8)
            throw new ArgumentException("Минимальная длина пароля — 8 символов.", nameof(length));

        const string validChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()_+-=[]{}|;:,.<>?";
        var randomBytes = new byte[length];

        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);

        return new string(randomBytes.Select(x => validChars[x % validChars.Length]).ToArray());
    }

    /// <summary>
    /// Проверка сложности пароля
    /// </summary>
    /// <param name="password">Пароль для проверки</param>
    /// <returns>Результат проверки сложности</returns>
    public bool IsPasswordComplex(string password)
    {
        return !string.IsNullOrWhiteSpace(password) && PasswordComplexityRegex.IsMatch(password);
    }
}

