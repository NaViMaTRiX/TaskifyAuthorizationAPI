using AuthAPI.DAL.Data;
using AuthAPI.DAL.Interfaces;
using AuthAPI.Domain.Models;
using AuthAPI.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace AuthAPI.DAL.Repository;

public class RefreshTokenRepository(AuthDbContext context) : IRefreshTokenRepository
{
    public async Task<RefreshToken> GetAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var token = await context.RefreshTokens.SingleOrDefaultAsync(rt => rt.Token == refreshToken, cancellationToken);
        
        if (token is null)
            throw new AggregateException("Refresh token not found");

        return token;
    }

    public async Task<RefreshToken> ExistsWithUserAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var existingRefreshToken = await context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken, cancellationToken);
        
        if(existingRefreshToken is null)
            throw new SecurityTokenException();

        return existingRefreshToken;
    }

    public async Task<bool> ExistsAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var token = await context.RefreshTokens.AnyAsync(rt => rt.Token == refreshToken, cancellationToken);

        if (token)
            throw new Microsoft.IdentityModel.Tokens.SecurityTokenException();
        
        return token;
    }

    public async Task<int> DeleteByUserAsync(User user, CancellationToken cancellationToken = default)
    {
         return await context.RefreshTokens
            .Where(rt => rt.UserId == user.Id && rt.IsRevoked)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task DeleteAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var token = await GetAsync(refreshToken, cancellationToken);

        if(token is null)
            throw new SecurityTokenException();

        context.RefreshTokens.Remove(token);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> SaveAsync(RefreshToken token, CancellationToken cancellationToken = default)
    {
        return await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<RefreshToken?> CreateAsync(RefreshToken newRefreshToken, CancellationToken cancellationToken = default)
    {
        if (newRefreshToken is null)
            throw new ArgumentNullException(nameof(newRefreshToken), "Переданный refresh-токен не может быть null");
        
        await context.RefreshTokens.AddAsync(newRefreshToken, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return newRefreshToken;
    }

    public async Task<int> RevokeTokensAsync(RefreshToken token, CancellationToken cancellationToken = default)
    {
        // 🔹 Отзываем все активные токены пользователя (оптимизированный SQL-запрос)
        var updatedTokens = await context.RefreshTokens
            .Where(rt => rt.UserId == token.UserId && !rt.IsRevoked) // Ищем активные токены пользователя
            .OrderByDescending(rt => rt.CreatedAt) // Сортируем по дате создания (или UpdatedAt)
            .Skip(1) // Пропускаем самый новый токен (оставляем его активным)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(rt => rt.IsRevoked, true)
                    .SetProperty(rt => rt.RevokedAt, DateTime.UtcNow),
                cancellationToken
            );

        return updatedTokens;
    }
}