using AuthAPI.Application.CQRS.Commands.User.CreateUser;
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
    Task<AuthResponse> RegisterAsync(CreateUserCommand userCommand, string? ipAddress, string userAgent, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Выход пользователя из системы
    /// </summary>
    Task LogoutAsync(LogoutRequest request, string? ipAddress,string userAgent, CancellationToken cancellationToken = default);
}
