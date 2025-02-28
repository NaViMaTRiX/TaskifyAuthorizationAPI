using AuthAPI.Domain.Enums;

namespace AuthAPI.Application.Dto;

/// <summary>
/// Статистика пользователя
/// </summary>
public class UserStatisticsDto
{
    /// <summary>
    /// Общее количество действий пользователя
    /// </summary>
    public int TotalActions { get; init; }

    /// <summary>
    /// Количество успешных входов
    /// </summary>
    public int SuccessfulLogins { get; init; }

    /// <summary>
    /// Количество неудачных входов
    /// </summary>
    public int FailedLogins { get; init; }

    /// <summary>
    /// Дата первого входа
    /// </summary>
    public DateTime? FirstLoginDate { get; init; }

    /// <summary>
    /// Дата последнего входа
    /// </summary>
    public DateTime? LastLoginDate { get; init; }

    /// <summary>
    /// Роль пользователя
    /// </summary>
    public UserRole Role { get; init; }

    /// <summary>
    /// Описание роли
    /// </summary>
    public string RoleDescription { get; init; } = string.Empty;

    /// <summary>
    /// Количество изменений роли
    /// </summary>
    public int RoleChangeCount { get; init; }

    /// <summary>
    /// Список последних действий
    /// </summary>
    public IEnumerable<UserAuditLogDto> RecentActions { get; init; }
}

/// <summary>
/// DTO для записи аудита
/// </summary>
public class UserAuditLogDto
{
    /// <summary>
    /// Тип действия
    /// </summary>
    public AuditAction Action { get; init; }

    /// <summary>
    /// Время действия
    /// </summary>
    public DateTime Timestamp { get; init; }

    /// <summary>
    /// Дополнительные детали
    /// </summary>
    public string? Details { get; init; }
}
