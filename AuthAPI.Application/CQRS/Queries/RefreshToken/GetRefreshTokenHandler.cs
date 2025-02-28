using AuthAPI.Application.Services;
using AuthAPI.DAL.Repository;
using AuthAPI.Shared.Exceptions;
using MediatR;

namespace AuthAPI.Application.CQRS.Queries.RefreshToken;

public record GetRefreshTokenRequest(string RefreshToken, string IpAddress) : IRequest<Domain.Models.RefreshToken>;


public class GetRefreshTokenHandler(RefreshTokenRepository tokenRepository, AuditService auditService)
    : IRequestHandler<GetRefreshTokenRequest, Domain.Models.RefreshToken>
{
    public async Task<Domain.Models.RefreshToken> Handle(GetRefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request));
        
        // 🔹 Разворачиваем объект `query` в локальные переменные
        var (refreshToken, ipAddress) = (request.RefreshToken, request.IpAddress);

        // 🔹 Проверка входных данных
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            await auditService.LogRefreshTokenFailureAsync(refreshToken, ipAddress, cancellationToken);
            throw new Exception("Токен не может быть пустым.");
        }
        
        var getRefreshToken = await tokenRepository.ExistsWithUserAsync(refreshToken, cancellationToken);

        // Валидация токена
        if (getRefreshToken is null || getRefreshToken.ExpiresAt < DateTime.UtcNow || getRefreshToken.IsRevoked)
        {
            await auditService.LogRefreshTokenFailureAsync(refreshToken, ipAddress, cancellationToken);
            throw new SecurityTokenException();
        }
                
        return getRefreshToken;
    }



}