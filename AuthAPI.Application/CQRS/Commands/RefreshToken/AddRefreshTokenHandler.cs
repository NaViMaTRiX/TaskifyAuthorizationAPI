using System.IdentityModel.Tokens.Jwt;
using AuthAPI.Application.Dto;
using AuthAPI.Application.Interface;
using AuthAPI.DAL.Data;

namespace AuthAPI.Application.CQRS.Commands.RefreshToken;

public class AddRefreshTokenHandler(ITokenService tokenService, AuthDbContext context)
{
    public async Task<TokenResponse> Handler(Domain.Models.User user, CancellationToken cancellationToken = default)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user), "Пользователь не должен быть null");

        // Генерация новых токенов
        var newJwtToken = await Task.Run(() => tokenService.GenerateJwtToken(user), cancellationToken);
        var newRefreshTokenString = await Task.Run(tokenService.GenerateRefreshToken, cancellationToken);

        var newRefreshToken = new Domain.Models.RefreshToken
        {
            Token = newRefreshTokenString,
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(14)
        };

        // Используем транзакцию, чтобы избежать несогласованных данных в БД
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await context.RefreshTokens.AddAsync(newRefreshToken, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        
            await transaction.CommitAsync(cancellationToken);

            return new TokenResponse
            {
                RefreshToken = newRefreshToken,
                JwtToken = newJwtToken,
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw new InvalidOperationException("User was not created.");
        }
    }

}
        
