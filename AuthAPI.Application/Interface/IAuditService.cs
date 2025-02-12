namespace AuthAPI.Application.Interface;

/// <summary>
/// Интерфейс сервиса аудита пользователей
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Записать событие входа в систему
    /// </summary>
    Task LogLoginAsync(
        Guid userId, 
        string? ipAddress, 
        bool success, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Записать событие выхода из системы
    /// </summary>
    Task LogLogoutAsync(
        Guid userId, 
        string? ipAddress, 
        bool allDevices, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Записать событие регистрации пользователя
    /// </summary>
    Task LogRegistrationAsync(
        Guid userId, 
        string? ipAddress, 
        bool success, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Записать событие неудачного обновления токена
    /// </summary>
    Task LogRefreshTokenFailureAsync(
        string refreshToken, 
        string? ipAddress,
        CancellationToken cancellationToken = default);
}
