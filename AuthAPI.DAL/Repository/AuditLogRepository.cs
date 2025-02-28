using AuthAPI.DAL.Data;
using AuthAPI.DAL.Interfaces;
using AuthAPI.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthAPI.DAL.Repository;

public class AuditLogRepository(AuthDbContext context) : IAuditLogRepository
{
    public async Task<UserAuditLog> CreateAsync(UserAuditLog userAuditLog, CancellationToken cancellationToken = default)
    {
        if (userAuditLog is null) 
            throw new ArgumentNullException(nameof(userAuditLog));
        
        await context.AddAsync(userAuditLog, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return userAuditLog;
    }

    public async Task<List<UserAuditLog>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var userLogs = await context.UserAuditLogs
            .AsNoTracking()
            .Where(log => log.UserId == userId)
            .OrderByDescending(log => log.Timestamp)
            .Take(20)
            .ToListAsync(cancellationToken);

        if (userLogs is null)
            throw new KeyNotFoundException();
        
        return userLogs;
    }
}