using AuthAPI.Application.CQRS.Queries.User;
using AuthAPI.Application.Dto;
using AuthAPI.Application.Services.Role;
using AuthAPI.DAL.Repository;

namespace AuthAPI.Application.CQRS.Queries.Role;

public class GetStatisticsByRoleHandler(UserRepo userRepo, CountAsyncHandler countHandler)
{
    public async Task<List<RoleStatisticsDto>> Handler(CancellationToken cancellationToken = default)
    {
        var totalUsers = await countHandler.Handler(cancellationToken);
        var userRole = await userRepo.GetUserRoleUserTable(cancellationToken);

        if (userRole is null)
            throw new KeyNotFoundException();

        var roleStatisticsDto =  userRole
            .Select(g => new RoleStatisticsDto
            {
                Role = g.Key,
                RoleDescription = RoleManagementService.GetRoleDescription(g.Key),
                UserCount = g.Count(),
                Percentage = totalUsers > 0 
                    ? Math.Round(g.Count() * 100.0 / totalUsers, 2) 
                    : 0
            })
            .OrderByDescending(r => r.UserCount)
            .ToList();

        return roleStatisticsDto;
    }
}