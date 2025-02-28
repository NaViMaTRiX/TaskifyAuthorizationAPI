using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthAPI.Domain.Models;

public class UserLoginActivity
{
    /// <summary>
    /// Уникальный идентификатор записи активности входа
    /// </summary>
    [Key]
    public Guid Id { get; init; }

    /// <summary>
    /// Идентификатор пользователя, связанного с этой активностью входа
    /// </summary>
    [ForeignKey(nameof(User))]
    public Guid UserId { get; init; }
    public User? User { get; init; }

    /// <summary>
    /// IP-адрес, с которого был выполнен вход (максимум 45 символов)
    /// </summary>
    [Required]
    [MaxLength(45)]
    public required string IpAddress { get; init; }

    /// <summary>
    /// Тип устройства, используемого для входа (максимум 50 символов)
    /// </summary>
    [MaxLength(50)]
    public required string DeviceType { get; init; }

    /// <summary>
    /// Браузер, используемый для входа (максимум 100 символов)
    /// </summary>
    [MaxLength(100)]
    public string? DeviceBrowser { get; init; }

    /// <summary>
    /// Операционная система устройства (максимум 100 символов)
    /// </summary>
    [MaxLength(100)]
    public string? DeviceOs { get; init; }

    /// <summary>
    /// Страна, из которой был выполнен вход (максимум 100 символов)
    /// </summary>
    [MaxLength(100)]
    public string? Country { get; init; }

    /// <summary>
    /// Город, из которого был выполнен вход (максимум 100 символов)
    /// </summary>
    [MaxLength(100)]
    public string? City { get; init; }

    /// <summary>
    /// Время входа в систему
    /// </summary>
    public DateTime LoginTime { get; init; }

    /// <summary>
    /// Время последней активности пользователя
    /// </summary>
    public DateTime? LastActivityTime { get; init; }

    /// <summary>
    /// Флаг, указывающий, активна ли текущая сессия
    /// </summary>
    public bool IsActive { get; init; }

    /// <summary>
    /// Флаг, указывающий, является ли вход подозрительным
    /// </summary>
    public bool IsSuspicious { get; init; }

    /// <summary>
    /// Количество неудачных попыток входа
    /// </summary>
    public int FailedLoginAttempts { get; init; }

    /// <summary>
    /// Уникальный отпечаток устройства (максимум 256 символов)
    /// </summary>
    [Required]
    [MaxLength(256)]
    public string? DeviceFingerprint { get; init; }
}
