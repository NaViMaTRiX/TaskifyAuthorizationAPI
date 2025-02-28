using System.ComponentModel.DataAnnotations;
using AuthAPI.Domain.Enums;

namespace AuthAPI.Application.Dto;

/// <summary>
/// Статистика распределения ролей
/// </summary>
public record RoleStatisticsDto
{
    /// <summary>
    /// Роль
    /// </summary>
    [Required]
    public UserRole Role { get; init; }

    /// <summary>
    /// Описание роли
    /// </summary>
    [Required]
    public string RoleDescription { get; init; } = string.Empty;

    /// <summary>
    /// Количество пользователей с этой ролью
    /// </summary>
    [Required]
    public int UserCount { get; init; }

    /// <summary>
    /// Процент пользователей с этой ролью
    /// </summary>
    [Required]
    public double Percentage { get; init; }
}
