namespace AuthAPI.Application.Interface;

/// <summary>
/// Интерфейс для работы с паролями
/// </summary>
public interface IPasswordService
{
    /// <summary>
    /// Хеширование пароля
    /// </summary>
    /// <param name="password">Исходный пароль</param>
    /// <returns>Захешированный пароль</returns>
    string HashPassword(string password);

    /// <summary>
    /// Проверка хэша пароль
    /// </summary>
    /// <param name="password">Пароль для проверки</param>
    /// <param name="hash">Хеш для сравнения</param>
    /// <returns>Результат проверки пароля</returns>
    bool VerifyPassword(string password, string hash);

    /// <summary>
    /// Генерация случайного пароля
    /// </summary>
    /// <param name="length">Длина пароля</param>
    /// <returns>Сгенерированный пароль</returns>
    string GenerateRandomPassword(int length = 12);

    /// <summary>
    /// Проверка сложности пароля
    /// </summary>
    /// <param name="password">Пароль для проверки</param>
    /// <returns>Результат проверки сложности</returns>
    bool IsPasswordComplex(string password);
}
