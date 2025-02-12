using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthAPI.Domain.Models;

public class UserLoginActivity
{
    /// <summary>
    /// Уникальный идентификатор записи активности входа
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Идентификатор пользователя, связанного с этой активностью входа
    /// </summary>
    [ForeignKey(nameof(User))]
    public Guid UserId { get; set; }
    public User User { get; set; }

    /// <summary>
    /// IP-адрес, с которого был выполнен вход (максимум 45 символов)
    /// </summary>
    [Required]
    [MaxLength(45)]
    public string IpAddress { get; set; }

    /// <summary>
    /// Тип устройства, используемого для входа (максимум 50 символов)
    /// </summary>
    [MaxLength(50)]
    public string DeviceType { get; set; }

    /// <summary>
    /// Браузер, используемый для входа (максимум 100 символов)
    /// </summary>
    [MaxLength(100)]
    public string DeviceBrowser { get; set; }

    /// <summary>
    /// Операционная система устройства (максимум 100 символов)
    /// </summary>
    [MaxLength(100)]
    public string DeviceOs { get; set; }

    /// <summary>
    /// Страна, из которой был выполнен вход (максимум 100 символов)
    /// </summary>
    [MaxLength(100)]
    public string Country { get; set; }

    /// <summary>
    /// Город, из которого был выполнен вход (максимум 100 символов)
    /// </summary>
    [MaxLength(100)]
    public string City { get; set; }

    /// <summary>
    /// Время входа в систему
    /// </summary>
    public DateTime LoginTime { get; set; }

    /// <summary>
    /// Время последней активности пользователя
    /// </summary>
    public DateTime? LastActivityTime { get; set; }

    /// <summary>
    /// Флаг, указывающий, активна ли текущая сессия
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Флаг, указывающий, является ли вход подозрительным
    /// </summary>
    public bool IsSuspicious { get; set; }

    /// <summary>
    /// Количество неудачных попыток входа
    /// </summary>
    public int FailedLoginAttempts { get; set; }

    /// <summary>
    /// Уникальный отпечаток устройства (максимум 256 символов)
    /// </summary>
    [Required]
    [MaxLength(256)]
    public string DeviceFingerprint { get; set; }
}
