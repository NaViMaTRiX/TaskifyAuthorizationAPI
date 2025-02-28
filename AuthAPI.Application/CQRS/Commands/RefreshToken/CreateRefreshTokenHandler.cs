using AuthAPI.Application.Dto;
using AuthAPI.Application.Interface;
using AuthAPI.Application.Mapping;
using AuthAPI.DAL.Interfaces;
using MediatR;

namespace AuthAPI.Application.CQRS.Commands.RefreshToken;

public record CreateRefreshTokenCommand(Domain.Models.User User) : IRequest<TokenResponse>;

public class CreateRefreshTokenHandler(ITokenService tokenService, IRefreshTokenRepository refreshTokenRepository)
        : IRequestHandler<CreateRefreshTokenCommand, TokenResponse>
{
    public async Task<TokenResponse> Handle(CreateRefreshTokenCommand command, CancellationToken cancellationToken = default)
    {
        if (command is null)
            throw new ArgumentNullException(nameof(command), "Пользователь не должен быть null");
        
        var user = command.User;

        // Генерация новых токенов
        var newJwtToken = await Task.Run(() => tokenService.GenerateJwtToken(user), cancellationToken);
        var newRefreshTokenString = await Task.Run(tokenService.GenerateRefreshToken, cancellationToken);

        //Мапинг пользователя в токены
        var newRefreshToken = user.ToRefreshTokenDto(newRefreshTokenString);
        
        await refreshTokenRepository.CreateAsync(newRefreshToken, cancellationToken);

        return new TokenResponse { JwtToken = newJwtToken, RefreshToken = newRefreshToken, };
    }
}
        
