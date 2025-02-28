using AuthAPI.Domain.Models;

namespace AuthAPI.DAL.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken> GetAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<RefreshToken> ExistsWithUserAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<int> DeleteByUserAsync(User user, CancellationToken cancellationToken = default);
    Task DeleteAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<int> SaveAsync(RefreshToken token, CancellationToken cancellationToken = default);
    Task<RefreshToken?> CreateAsync(RefreshToken token, CancellationToken cancellationToken = default);
    Task<int> RevokeTokensAsync(RefreshToken token, CancellationToken cancellationToken = default);
}