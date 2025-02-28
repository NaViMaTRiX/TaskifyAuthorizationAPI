using System.Security.Claims;
using AuthAPI.Application.Dto;
using AuthAPI.Domain.Models;

namespace AuthAPI.Application.Interface;

/// <summary>
/// Интерфейс для работы с токенами аутентификации
/// </summary>
public interface   ITokenService
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
    /// <returns>AuthResponse или пользователя с токенами</returns>
    /// </summary>
    Task<AuthResponse> GenerateAuthResponseAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Проверка и извлечение claims из токена
    /// </summary>
    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);

    /// <summary>
    /// Обновление токенов по refresh токену
    /// </summary>
    Task<AuthResponse> RefreshTokenAsync(string refreshToken, string ipAddress, CancellationToken cancellationToken = default);

    /// <summary>
    /// Удаление refresh токена
    /// </summary>
    Task DeleteRefreshTokenAsync(string refreshToken, string ipAddress, CancellationToken cancellationToken = default);

    /// <summary>
    /// Извлечение claims из JWT токена
    /// </summary>
    JwtTokenClaimsDto ExtractTokenClaims(string token, CancellationToken cancellationToken = default);
}
