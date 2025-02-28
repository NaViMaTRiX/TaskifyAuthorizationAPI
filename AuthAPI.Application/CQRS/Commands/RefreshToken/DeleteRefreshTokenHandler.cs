using AuthAPI.DAL.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthAPI.Application.CQRS.Commands.RefreshToken;

public record DeleteRefreshTokenCommand(Domain.Models.User User) : IRequest<Unit>;

public class DeleteRefreshTokenHandler(IRefreshTokenRepository tokenRepository, ILogger<DeleteRefreshTokenHandler> logger)
        : IRequestHandler<DeleteRefreshTokenCommand, Unit>
{
    public async Task<Unit> Handle(DeleteRefreshTokenCommand command, CancellationToken cancellationToken = default)
    {
        if (command is null)
        {
            logger.LogError("Попытка удаления токенов с `null` пользователем.");
            throw new ArgumentNullException(nameof(command), "Пользователь не должен быть null");
        }

        var user = command.User;
        try
        {
            await tokenRepository.DeleteByUserAsync(user, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при удалении старых refresh-токенов пользователя {UserId}.", user?.Id);
            throw; // Пробрасываем исключение дальше
        }
        return Unit.Value;
    }
}