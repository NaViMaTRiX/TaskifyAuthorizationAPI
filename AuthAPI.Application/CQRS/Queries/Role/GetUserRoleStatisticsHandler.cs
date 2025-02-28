using AuthAPI.DAL.Interfaces;
using AuthAPI.Domain.Enums;
using MediatR;

namespace AuthAPI.Application.CQRS.Queries.Role;

public record GetUserRoleStatisticsRequest() : IRequest<Dictionary<UserRole, int>>;

public class GetUserRoleStatisticsHandler(IRoleRepository roleRepository) 
    : IRequestHandler<GetUserRoleStatisticsRequest, Dictionary<UserRole, int>>
{
    /// <summary>
    /// Получить количество пользователей с каждой ролью
    /// </summary>
    /// <param name="request">Запрос(В нём ничего)</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Dictionary<UserRole, int>> Handle(GetUserRoleStatisticsRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null) 
            throw new ArgumentNullException(nameof(request));
        
        var userRole = await roleRepository.AllRolesGroupByAsync(cancellationToken);

        if (userRole is null)
            throw new KeyNotFoundException();

        if (userRole is null)
            throw new KeyNotFoundException();

        var userStats = userRole
            .Select(g => new { Role = g.Key, Count = g.Count() })
            .ToDictionary(x => x.Role, x => x.Count);

        return userStats;
    }
}