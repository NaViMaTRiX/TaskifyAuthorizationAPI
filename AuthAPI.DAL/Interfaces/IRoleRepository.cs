using AuthAPI.Domain.Enums;
using AuthAPI.Domain.Models;

namespace AuthAPI.DAL.Interfaces;

public interface IRoleRepository
{
    Task <UserRole> UpdateByUserAsync(User user, UserRole role, CancellationToken cancellationToken = default);
    Task<List<IGrouping<UserRole, User>>?> AllRolesGroupByAsync(CancellationToken cancellationToken = default);
}