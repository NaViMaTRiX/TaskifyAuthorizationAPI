using AuthAPI.Domain.Enums;
using AuthAPI.Domain.Models;

namespace AuthAPI.DAL.Interfaces;

public interface IUser
{
    /// <summary>
    /// Return user by role table
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<List<IGrouping<UserRole, User>>> GetUserRoleUserTable(CancellationToken cancellationToken = default);
}