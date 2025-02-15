using AuthAPI.DAL.Data;
using AuthAPI.DAL.Interfaces;
using AuthAPI.Domain.Enums;
using AuthAPI.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthAPI.DAL.Repository;

public abstract class UserRepository(AuthDbContext context) : IUser
{
    public async Task<List<IGrouping<UserRole, User>>> GetUserRoleUserTable(CancellationToken cancellationToken = default)
    {
        var roleUser = await context.Users
            .GroupBy(u => u.Role)
            .ToListAsync(cancellationToken);

        if (roleUser is null)
            throw new KeyNotFoundException();

        return roleUser;
    }
}