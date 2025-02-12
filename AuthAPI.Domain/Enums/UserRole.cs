using System.ComponentModel;

namespace AuthAPI.Domain.Enums;

/// <summary>
/// Роли пользователей в системе
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Гость с минимальными правами
    /// </summary>
    [Description("Гость")]
    Guest = 0,

    /// <summary>
    /// Обычный пользователь с базовыми правами
    /// </summary>
    [Description("Пользователь")]
    User = 1,

    /// <summary>
    /// Модератор с правами управления контентом
    /// </summary>
    [Description("Модератор")]
    Moderator = 2,

    /// <summary>
    /// Администратор с правами управления системой
    /// </summary>
    [Description("Администратор")]
    Admin = 3,

    /// <summary>
    /// Технический администратор с максимальными правами
    /// </summary>
    [Description("Технический администратор")]
    SuperAdmin = 4
}
