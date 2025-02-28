using AuthAPI.DAL.Data;
using AuthAPI.DAL.Interfaces;
using AuthAPI.Domain.Enums;
using AuthAPI.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthAPI.DAL.Repository;

public class RoleRepository(AuthDbContext context) : IRoleRepository
{
    public async Task<UserRole> UpdateByUserAsync(User user, UserRole newRole, CancellationToken cancellationToken = default)
    {
        var oldRole = user.Role;
        user.Role = newRole;
        
        await context.SaveChangesAsync(cancellationToken);
        return oldRole;
    }

    public async Task<List<IGrouping<UserRole,User>>?> AllRolesGroupByAsync(CancellationToken cancellationToken = default)
    {
        var userRole = await context.Users
            .GroupBy(u => u.Role)
            .ToListAsync(cancellationToken);
        
        if(userRole is null)
            throw new KeyNotFoundException();

        return userRole;
    }
}