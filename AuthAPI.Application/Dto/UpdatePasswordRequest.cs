namespace AuthAPI.Application.Dto;

/// <summary>
/// Запрос на обновление пароля
/// </summary>
public record UpdatePasswordRequest
{
    /// <summary>
    /// Текущий пароль пользователя
    /// </summary>
    public required string CurrentPassword { get; init; }
    
    /// <summary>
    /// Новый пароль пользователя
    /// </summary>
    public required string NewPassword { get; init; }
}
