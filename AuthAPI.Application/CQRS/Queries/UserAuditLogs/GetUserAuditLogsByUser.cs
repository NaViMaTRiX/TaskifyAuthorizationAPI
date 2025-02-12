using AuthAPI.DAL.Data;
using AuthAPI.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthAPI.Application.CQRS.Queries.UserAuditLogs;

public class GetUserAuditLogsByUser(AuthDbContext context)
{
    public async Task<List<UserAuditLog>> Handler(Guid userId, CancellationToken cancellationToken = default)
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