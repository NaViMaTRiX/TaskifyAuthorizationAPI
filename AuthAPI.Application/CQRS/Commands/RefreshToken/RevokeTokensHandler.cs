using AuthAPI.Application.Dto;
using AuthAPI.DAL.Data;
using AuthAPI.DAL.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthAPI.Application.CQRS.Commands.RefreshToken;

public record RevokeTokensRequest(LogoutRequest Request, Domain.Models.RefreshToken Token) : IRequest<Unit>;

public class RevokeTokensHandler(AuthDbContext context, IRefreshTokenRepository repository, ILogger<RevokeTokensHandler> logger)
    : IRequestHandler<RevokeTokensRequest, Unit>
{
    public async Task<Unit> Handle(RevokeTokensRequest request, CancellationToken cancellationToken = default)
    {
        // 🔹 Валидация входных данных
        if (request is null)
        {
            logger.LogError("Ошибка отзыва токенов: входной запрос `RevokeTokenRequest` равен `null`.");
            throw new ArgumentNullException(nameof(request), "Запрос на отзыв токенов не может быть null");
        }
        
        var (token, logoutFromAllDevices) = (request.Token, request.Request.LogoutFromAllDevices);

        if (token is null)
        {
            logger.LogError("Ошибка отзыва токенов: переданный `RefreshToken` равен `null`.");
            throw new ArgumentNullException(nameof(token), "Переданный refresh-токен не может быть null");
        }
        
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            if (logoutFromAllDevices)
            {
                var updatedTokens = await repository.RevokeTokensAsync(token, cancellationToken);
                logger.LogInformation("Отозвано {Count} токенов для пользователя {UserId}.", updatedTokens, token.UserId);
            }

            // 🔹 Отзываем текущий токен
            token.Revoke();

            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            logger.LogInformation("Токен пользователя {UserId} отозван успешно.", token.UserId);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            logger.LogError(ex, "Ошибка при отзыве токенов пользователя {UserId}.", token?.UserId);
            throw;
        }
        return Unit.Value;
    }
}