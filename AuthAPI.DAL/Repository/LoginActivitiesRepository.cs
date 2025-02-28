using AuthAPI.DAL.Data;
using AuthAPI.DAL.Interfaces;
using AuthAPI.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthAPI.DAL.Repository;

public class LoginActivitiesRepository(AuthDbContext context) : ILoginActivitiesRepository
{
    public async Task<List<UserLoginActivity>> GetLastLoginsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var recentLogins = await context.UserLoginActivities
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.LoginTime)
            .Take(5)
            .ToListAsync(cancellationToken);
        
        return recentLogins;
    }

    public async Task<int> GetFailedAttemptsCount(Guid userId, string ipAddress, DateTime cutoffTime,
        CancellationToken cancellationToken = default)
    {
        return await context.UserLoginActivities
            .AsNoTracking()
            .Where(x => x.UserId == userId &&
                        x.IpAddress == ipAddress &&
                        x.LoginTime > cutoffTime)
            .CountAsync(cancellationToken);
    }

    public async Task<UserLoginActivity> CreateActivityAsync(UserLoginActivity loginActivity, CancellationToken cancellationToken = default)
    {
        if (loginActivity is null)
            throw new ArgumentNullException();
        
        await context.UserLoginActivities.AddAsync(loginActivity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return loginActivity;
    }
}