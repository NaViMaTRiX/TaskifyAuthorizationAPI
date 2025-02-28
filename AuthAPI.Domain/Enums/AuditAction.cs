using System.ComponentModel;

namespace AuthAPI.Domain.Enums;

/// <summary>
/// Типы действий для аудита пользователей
/// </summary>
public enum AuditAction
{
    /// <summary>
    /// Успешный вход в систему
    /// </summary>
    [Description("Успешный вход в систему")]
    LoginSuccess = 1,

    /// <summary>
    /// Неудачная попытка входа
    /// </summary>
    [Description("Неудачная попытка входа")]
    LoginFailed = 2,

    /// <summary>
    /// Изменение роли пользователя
    /// </summary>
    [Description("Изменение роли пользователя")]
    RoleChanged = 3,

    /// <summary>
    /// Успешная регистрация
    /// </summary>
    [Description("Успешная регистрация")]
    RegistrationSuccess = 4,

    /// <summary>
    /// Неудачная регистрация
    /// </summary>
    [Description("Неудачная регистрация")]
    RegistrationFailed = 5,

    /// <summary>
    /// Выход из системы
    /// </summary>
    [Description("Выход из системы")]
    Logout = 6,

    /// <summary>
    /// Выход из всех устройств
    /// </summary>
    [Description("Выход из всех устройств")]
    LogoutAllDevices = 7,

    /// <summary>
    /// Неудачная попытка обновления токена
    /// </summary>
    [Description("Неудачная попытка обновления токена")]
    RefreshTokenFailed = 8,

    /// <summary>
    /// Создание нового пользователя
    /// </summary>
    [Description("Создание нового пользователя")]
    UserCreated = 9,
    
    /// <summary>
    /// Обновление пользователя
    /// </summary>
    [Description("Обновление пользователя")]
    UserUpdate = 10,

    /// <summary>
    /// Удаление пользователя
    /// </summary>
    [Description("Удаление пользователя")]
    UserDeleted = 11,

    /// <summary>
    /// Обновление профиля
    /// </summary>
    [Description("Обновление профиля")]
    ProfileUpdated = 12
}
