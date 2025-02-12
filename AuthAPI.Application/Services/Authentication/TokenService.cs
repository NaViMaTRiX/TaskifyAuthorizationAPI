using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AuthAPI.Application.Dto;
using AuthAPI.Application.Interface;
using AuthAPI.Application.Services.Role;
using AuthAPI.DAL.Data;
using AuthAPI.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SecurityTokenException = AuthAPI.Shared.Exceptions.SecurityTokenException;

namespace AuthAPI.Application.Services.Authentication;

/// <summary>
/// Сервис для работы с токенами аутентификации
/// </summary>
public class TokenService(AuthDbContext context, IConfiguration configuration) : ITokenService
{
    /// <inheritdoc/>
    public string GenerateJwtToken(Domain.Models.User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"] ?? string.Empty));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.GivenName, user.FirstName),
            new Claim(ClaimTypes.Surname, user.LastName),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(3),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <inheritdoc/>
    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    /// <inheritdoc/>
    public async Task<AuthResponse> GenerateAuthResponseAsync(Domain.Models.User user, CancellationToken cancellationToken = default)
    {
        var token = GenerateJwtToken(user);
        var refreshTokenString = GenerateRefreshToken();
        
        var refreshToken = new RefreshToken
        {
            Token = refreshTokenString,
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(14)
        };
        //try catch
        context.RefreshTokens.Add(refreshToken);
        await context.SaveChangesAsync(cancellationToken);

        return new AuthResponse
        {
            Token = token,
            RefreshToken = refreshTokenString,
            ExpiresAt = DateTime.UtcNow.AddDays(3),
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = RoleManagementService.GetRoleDescription(user.Role)
            }
        };
    }

    /// <summary>
    /// Проверка и извлечение claims из токена
    /// </summary>
    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"] ?? string.Empty)),
            ValidateLifetime = false, // Важно! Отключаем проверку времени жизни
            ValidIssuer = configuration["Jwt:Issuer"],
            ValidAudience = configuration["Jwt:Audience"]
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        
        try 
        {
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
            
            // Проверяем, что токен корректного типа
            if (securityToken is not JwtSecurityToken jwtToken || 
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException();
            }

            return principal;
        }
        catch (Exception)
        {
            throw new SecurityTokenException();
        }
    }

    /// <summary>
    /// Проверка валидности токена
    /// </summary>
    public bool IsJwtTokenExpired(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;
        
        return jwtToken?.ValidTo < DateTime.UtcNow;
    }

    /// <inheritdoc/>
    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        // Проверяем наличие refresh токена в базе данных
        var existingRefreshToken = await context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken, cancellationToken);

        // Если токен не найден или просрочен - возвращаем ошибку
        if (existingRefreshToken == null || existingRefreshToken.ExpiresAt < DateTime.UtcNow)
        {
            throw new SecurityTokenException();
        }

        var user = existingRefreshToken.User;

        // Удаляем старый refresh токен
        context.RefreshTokens.Remove(existingRefreshToken);

        // Генерируем новые токены
        var newJwtToken = GenerateJwtToken(user);
        var newRefreshTokenString = GenerateRefreshToken();

        var newRefreshToken = new RefreshToken
        {
            Token = newRefreshTokenString,
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(14)
        };

        // Сохраняем новый refresh токен
        context.RefreshTokens.Add(newRefreshToken);
        await context.SaveChangesAsync(cancellationToken);

        // Возвращаем новые токены
        return new AuthResponse
        {
            Token = newJwtToken,
            RefreshToken = newRefreshTokenString,
            ExpiresAt = DateTime.UtcNow.AddDays(3),
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = RoleManagementService.GetRoleDescription(user.Role)
            }
        };
    }

    /// <inheritdoc/>
    public async Task DeleteJwtTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        // В текущей реализации JWT токены не хранятся в базе данных,
        // поэтому удаление происходит на клиентской стороне
        // Здесь можно добавить дополнительную логику инвалидации токена, если потребуется
        await Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async Task DeleteRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var token = await context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken, cancellationToken);

        if (token != null)
        {
            context.RefreshTokens.Remove(token);
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    /// <inheritdoc/>
    public Task<JwtTokenClaimsDto> ExtractTokenClaimsAsync(string token, CancellationToken cancellationToken = default)
    {
        try 
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            if (tokenHandler.ReadToken(token) is not JwtSecurityToken jwtToken)
            {
                throw new SecurityTokenException();
            }

            return Task.FromResult(new JwtTokenClaimsDto
            {
                UserId = Guid.Parse(jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString()),
                Email = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value,
                FirstName = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value,
                LastName = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname)?.Value,
                Role = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value
            });
        }
        catch (Exception)
        {
            throw new SecurityTokenException();
        }
    }
}
