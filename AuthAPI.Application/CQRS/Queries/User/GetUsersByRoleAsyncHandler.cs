using AuthAPI.DAL.Data;
using AuthAPI.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AuthAPI.Application.CQRS.Queries.User;

public class GetUsersByRoleAsyncHandler(AuthDbContext context)
{
    /// <summary>
    /// Return user by role
    /// </summary>
    /// <param name="role">Role type UserRole</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns></returns>
    public async Task<List<Domain.Models.User>> Handler(UserRole role, CancellationToken cancellationToken = default)
    {
        var users = await context.Users
            .AsNoTracking()
            .Where(u => u.Role == role)
            .ToListAsync(cancellationToken);
        
        if (users is null) //TODO: надо сделать Exception отдельный
            throw new KeyNotFoundException();

        return users;
    }
}