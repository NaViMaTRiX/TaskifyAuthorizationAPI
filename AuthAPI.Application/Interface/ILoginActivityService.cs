namespace AuthAPI.Application.Interface;

public interface ILoginActivityService
{
    /// <summary>
    /// Проверка возможности входа для пользователя
    /// </summary>
    Task<bool> IsLoginAllowedAsync(
        Guid userId, 
        string ipAddress, 
        string deviceFingerprint, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Логирование успешного входа
    /// </summary>
    Task RecordSuccessfulLoginAsync(
        Guid userId, 
        string ipAddress, 
        string deviceInfo, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Логирование неудачной попытки входа
    /// </summary>
    Task RecordFailedLoginAsync(
        Guid userId, 
        string ipAddress, 
        string deviceInfo, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Проверка подозрительности входа
    /// </summary>
    Task<bool> IsSuspiciousLoginAsync(
        Guid userId, 
        string ipAddress, 
        string deviceFingerprint, 
        CancellationToken cancellationToken = default);
}
