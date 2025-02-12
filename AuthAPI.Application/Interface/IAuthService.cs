using AuthAPI.Application.Dto;

namespace AuthAPI.Application.Interface;

/// <summary>
/// Интерфейс сервиса аутентификации пользователей
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Вход пользователя в систему
    /// </summary>
    Task<AuthResponse> LoginAsync(LoginRequest request, string? ipAddress, string userAgent, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Регистрация нового пользователя
    /// </summary>
    Task<AuthResponse> RegisterAsync(RegisterRequest request, string? ipAddress, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Обновление токена доступа
    /// </summary>
    Task<AuthResponse> RefreshTokenAsync(string refreshToken,string? ipAddress, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Выход пользователя из системы
    /// </summary>
    Task LogoutAsync(LogoutRequest request, string? ipAddress,string userAgent, CancellationToken cancellationToken = default);
}
