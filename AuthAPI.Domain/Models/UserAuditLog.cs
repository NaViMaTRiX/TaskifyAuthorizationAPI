using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AuthorizationAPI.Domain.Enums;

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
    public long Id { get; set; }

    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    [Required]
    public Guid UserId { get; set; }

    /// <summary>
    /// Тип действия
    /// </summary>
    [Required]
    public AuditAction Action { get; set; }

    /// <summary>
    /// IP-адрес, с которого выполнено действие
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// Дополнительные детали действия
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// Временная метка действия
    /// </summary>
    [Required]
    public DateTime Timestamp { get; set; }
}
