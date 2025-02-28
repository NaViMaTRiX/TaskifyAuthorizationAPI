using AuthAPI.Domain.Enums;
using AuthAPI.Domain.Models;

namespace AuthAPI.DAL.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByRefreshTokenAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> CreateAsync(User user, CancellationToken cancellationToken = default);
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string email, CancellationToken cancellationToken = default);
    Task<List<User?>> GetByRoleAsync(UserRole role, CancellationToken cancellationToken = default);
    Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task UpdateLastTimeAsync(User user, CancellationToken cancellationToken = default);
    //Task List<> GetRoleByUserAsync(User user, CancellationToken cancellationToken = default);
}