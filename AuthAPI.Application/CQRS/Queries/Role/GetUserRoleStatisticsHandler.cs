using AuthAPI.DAL.Repository;
using AuthAPI.Domain.Enums;

namespace AuthAPI.Application.CQRS.Queries.Role;

public class GetUserRoleStatisticsHandler(UserRepository userRepository)
{
    /// <summary>
    /// Получить количество пользователей с каждой ролью
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Dictionary<UserRole, int>> Handler(CancellationToken cancellationToken = default)
    {
        var userRole = await userRepository.GetUserRoleUserTable(cancellationToken);

        if(userRole is null)
            throw new KeyNotFoundException();
        
        var userStats =  userRole
            .Select(g => new { Role = g.Key, Count = g.Count() })
            .ToDictionary(x => x.Role, x => x.Count);
        
        return userStats;
    }
}