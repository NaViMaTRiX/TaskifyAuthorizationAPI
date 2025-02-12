namespace AuthorizationAPI.Domain.Enums;

/// <summary>
/// Типы действий для аудита пользователей
/// </summary>
public enum AuditAction
{
    /// <summary>
    /// Успешный вход в систему
    /// </summary>
    LoginSuccess = 1,

    /// <summary>
    /// Неудачная попытка входа
    /// </summary>
    LoginFailed = 2,

    /// <summary>
    /// Изменение роли пользователя
    /// </summary>
    RoleChanged = 3,

    /// <summary>
    /// Успешная регистрация
    /// </summary>
    RegistrationSuccess = 4,

    /// <summary>
    /// Неудачная регистрация
    /// </summary>
    RegistrationFailed = 5,

    /// <summary>
    /// Выход из системы
    /// </summary>
    Logout = 6,

    /// <summary>
    /// Выход из всех устройств
    /// </summary>
    LogoutAllDevices = 7,

    /// <summary>
    /// Неудачная попытка обновления токена
    /// </summary>
    RefreshTokenFailed = 8,

    /// <summary>
    /// Создание нового пользователя
    /// </summary>
    UserCreated = 9,

    /// <summary>
    /// Удаление пользователя
    /// </summary>
    UserDeleted = 10,

    /// <summary>
    /// Обновление профиля
    /// </summary>
    ProfileUpdated = 11
}
