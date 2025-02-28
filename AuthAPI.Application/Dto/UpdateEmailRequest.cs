namespace AuthAPI.Application.Dto;

/// <summary>
/// Запрос на обновление email
/// </summary>
public record UpdateEmailRequest
{
    /// <summary>
    /// Текущий пароль пользователя
    /// </summary>
    public required string CurrentPassword { get; init; }
    
    /// <summary>
    /// Новый email пользователя
    /// </summary>
    public required string Email { get; init; }
}
