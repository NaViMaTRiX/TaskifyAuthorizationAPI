using System.ComponentModel.DataAnnotations;
using AuthAPI.Domain.Enums;
using AuthorizationAPI.Domain.Enums;

namespace AuthAPI.Application.Dto;

/// <summary>
/// Статистика распределения ролей
/// </summary>
public class RoleStatisticsDto
{
    /// <summary>
    /// Роль
    /// </summary>
    [Required]
    public UserRole Role { get; set; }

    /// <summary>
    /// Описание роли
    /// </summary>
    [Required]
    public string RoleDescription { get; set; } = string.Empty;

    /// <summary>
    /// Количество пользователей с этой ролью
    /// </summary>
    [Required]
    public int UserCount { get; set; }

    /// <summary>
    /// Процент пользователей с этой ролью
    /// </summary>
    [Required]
    public double Percentage { get; set; }
}
