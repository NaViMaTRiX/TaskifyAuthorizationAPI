using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AuthAPI.Application.CQRS.Commands.RefreshToken;
using AuthAPI.Application.CQRS.Queries.RefreshToken;
using AuthAPI.Application.Dto;
using AuthAPI.Application.Interface;
using AuthAPI.Application.Mapping;
using AuthAPI.DAL.Data;
using AuthAPI.DAL.Interfaces;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SecurityTokenException = AuthAPI.Shared.Exceptions.SecurityTokenException;

namespace AuthAPI.Application.Services.Authentication;

/// <summary>
/// Сервис для работы с токенами аутентификации
/// </summary>
public class TokenService(AuthDbContext context,
    IRefreshTokenRepository refreshTokenRepository,
    IConfiguration configuration,
    IMediator mediator,
    ILogger<TokenService> logger) : ITokenService
{

    /// <inheritdoc/>
    public string GenerateJwtToken(Domain.Models.User user)
    {
        if (user is null)
        {
            logger.LogError("Ошибка: пользователь null при генерации JWT токена.");
            throw new ArgumentNullException(nameof(user), "Пользователь не может быть null.");
        }

        var key = Encoding.UTF8.GetBytes(configuration.GetRequiredSection("Jwt:Key").Value!);
        var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.GivenName, user.FirstName ?? string.Empty),
            new Claim(ClaimTypes.Surname, user.LastName ?? string.Empty),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: configuration.GetRequiredSection("Jwt:Issuer").Value,
            audience: configuration.GetRequiredSection("Jwt:Audience").Value,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(3),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <inheritdoc/>
    public string GenerateRefreshToken()
    {
        int tokenSizeInBytes = 32;

        Span<byte> randomNumber = stackalloc byte[tokenSizeInBytes];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
    
        // URL-safe Base64 encoding without padding
        return Convert.ToBase64String(randomNumber)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }

    /// <inheritdoc/>
    public async Task<AuthResponse> GenerateAuthResponseAsync(Domain.Models.User user, CancellationToken cancellationToken = default)
    {
        var tokens = await mediator.Send(new CreateRefreshTokenCommand(user),cancellationToken);
        
        //Мапим пользователя в ответ
        return user.ToWithAuthResponse(tokens.JwtToken, tokens.RefreshToken.Token);
    }

    /// <inheritdoc/>
    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!)),
            ValidateLifetime = false,
            ValidIssuer = configuration["Jwt:Issuer"],
            ValidAudience = configuration["Jwt:Audience"]
        };

        try
        {
            return tokenHandler.ValidateToken(token, tokenValidationParameters, out _);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при валидации токена.");
            throw new SecurityTokenException();
        }
    }

    /// <inheritdoc/>
    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken, string? ipAddress, CancellationToken cancellationToken = default)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            // Проверяем наличие refresh токена в базе данных
            var existingRefreshToken = await mediator.Send(new GetRefreshTokenRequest(refreshToken, ipAddress!), cancellationToken);
            
            var user = existingRefreshToken.User!;
            
            // Деактивируем старый refresh токен пользователя
            existingRefreshToken.Revoke();
            await context.SaveChangesAsync(cancellationToken);

            // Удаляем старые токены пользователя? Хз может и не надо просто чистить раз в 2 месяца
            await mediator.Send(new DeleteRefreshTokenCommand(user), cancellationToken);

            var tokens = await mediator.Send(new CreateRefreshTokenCommand(user),cancellationToken);

            // Фиксируем транзакцию
            await transaction.CommitAsync(cancellationToken);
            
            // Возвращаем пользователя с новыми токенами
            return tokens.ToAuthResponse(user);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task DeleteRefreshTokenAsync(string refreshToken, string ipAddress, CancellationToken cancellationToken = default)
    {
        await refreshTokenRepository.DeleteAsync(refreshToken, cancellationToken);
    }

    /// <inheritdoc/>
    public JwtTokenClaimsDto ExtractTokenClaims(string token, CancellationToken cancellationToken = default)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        if (tokenHandler.ReadToken(token) is not JwtSecurityToken jwtToken)
            throw new SecurityTokenException();

        return jwtToken.ToJwtTokenClaimsDto();
    }
}

