using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AuthAPI.Domain.Enums;

namespace AuthAPI.Domain.Models;

/// <summary>
/// Модель журнала аудита действий пользователей
/// </summary>
public class UserAuditLog
{
    /// <summary>
    /// Уникальный идентификатор записи аудита
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; init; }

    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public Guid? UserId { get; init; }

    /// <summary>
    /// Тип действия
    /// </summary>
    [Required]
    public AuditAction Action { get; init; }

    /// <summary>
    /// IP-адрес, с которого выполнено действие
    /// </summary>
    public string? IpAddress { get; init; }

    /// <summary>
    /// Дополнительные детали действия
    /// </summary>
    public string? Details { get; init; }

    /// <summary>
    /// Временная метка действия
    /// </summary>
    [Required]
    public DateTime Timestamp { get; init; }
}
