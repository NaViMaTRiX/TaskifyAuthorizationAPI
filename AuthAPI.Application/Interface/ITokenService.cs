using System.Security.Claims;
using AuthAPI.Application.Dto;
using AuthAPI.Domain.Models;

namespace AuthAPI.Application.Interface;

/// <summary>
/// Интерфейс для работы с токенами аутентификации
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Генерация JWT токена для пользователя
    /// </summary>
    string GenerateJwtToken(User user);

    /// <summary>
    /// Генерация refresh токена
    /// </summary>
    string GenerateRefreshToken();

    /// <summary>
    /// Создание полного ответа аутентификации с токенами
    /// </summary>
    Task<AuthResponse> GenerateAuthResponseAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Проверка и извлечение claims из токена
    /// </summary>
    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);

    /// <summary>
    /// Проверка валидности токена
    /// </summary>
    bool IsJwtTokenExpired(string token);

    /// <summary>
    /// Обновление токенов по refresh токену
    /// </summary>
    Task<AuthResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Удаление JWT токена
    /// </summary>
    Task DeleteJwtTokenAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Удаление refresh токена
    /// </summary>
    Task DeleteRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Извлечение claims из JWT токена
    /// </summary>
    Task<JwtTokenClaimsDto> ExtractTokenClaimsAsync(string token, CancellationToken cancellationToken = default);
}
