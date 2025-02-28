using AuthAPI.Domain.Models;

namespace AuthAPI.DAL.Interfaces;

public interface IAuditLogRepository
{
    Task<UserAuditLog> CreateAsync(UserAuditLog userAuditLog, CancellationToken cancellationToken = default);
    Task<List<UserAuditLog>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}