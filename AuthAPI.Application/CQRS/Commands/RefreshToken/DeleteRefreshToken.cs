using AuthAPI.DAL.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AuthAPI.Application.CQRS.Commands.RefreshToken;

public class DeleteRefreshToken(AuthDbContext context, ILogger logger)
{
    public async Task Handler(Domain.Models.User user, CancellationToken cancellationToken = default)
    {
        if (user == null)
        {
            logger.LogError("Попытка удаления токенов с `null` пользователем.");
            throw new ArgumentNullException(nameof(user), "Пользователь не должен быть null");
        }

        try
        {
            await context.RefreshTokens
                .Where(rt => rt.UserId == user.Id && rt.IsRevoked)
                .ExecuteDeleteAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при удалении старых refresh-токенов пользователя {UserId}.", user?.Id);
            throw; // Пробрасываем исключение дальше
        }
    }
}