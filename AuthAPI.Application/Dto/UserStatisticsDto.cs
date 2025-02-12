using AuthAPI.Domain.Enums;
using AuthorizationAPI.Domain.Enums;

namespace AuthAPI.Application.Dto;

/// <summary>
/// Статистика пользователя
/// </summary>
public class UserStatisticsDto
{
    /// <summary>
    /// Общее количество действий пользователя
    /// </summary>
    public int TotalActions { get; set; }

    /// <summary>
    /// Количество успешных входов
    /// </summary>
    public int SuccessfulLogins { get; set; }

    /// <summary>
    /// Количество неудачных входов
    /// </summary>
    public int FailedLogins { get; set; }

    /// <summary>
    /// Дата первого входа
    /// </summary>
    public DateTime? FirstLoginDate { get; set; }

    /// <summary>
    /// Дата последнего входа
    /// </summary>
    public DateTime? LastLoginDate { get; set; }

    /// <summary>
    /// Роль пользователя
    /// </summary>
    public UserRole Role { get; set; }

    /// <summary>
    /// Описание роли
    /// </summary>
    public string RoleDescription { get; set; } = string.Empty;

    /// <summary>
    /// Количество изменений роли
    /// </summary>
    public int RoleChangeCount { get; set; }

    /// <summary>
    /// Список последних действий
    /// </summary>
    public List<UserAuditLogDto>? RecentActions { get; set; }
}

/// <summary>
/// DTO для записи аудита
/// </summary>
public class UserAuditLogDto
{
    /// <summary>
    /// Тип действия
    /// </summary>
    public AuditAction Action { get; set; }

    /// <summary>
    /// Время действия
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Дополнительные детали
    /// </summary>
    public string? Details { get; set; }
}
