using AuthAPI.Application.CQRS.Queries.User;
using AuthAPI.Application.Dto;
using AuthAPI.Application.Mapping;
using AuthAPI.DAL.Interfaces;
using MediatR;

namespace AuthAPI.Application.CQRS.Queries.Role;

public record GetStatisticsByRoleRequest() : IRequest<List<RoleStatisticsDto>>;

public class GetStatisticsByRoleHandler(IRoleRepository roleRepository, CountAsyncHandler countHandler) 
    : IRequestHandler<GetStatisticsByRoleRequest, List<RoleStatisticsDto>>
{
    public async Task<List<RoleStatisticsDto>> Handle(GetStatisticsByRoleRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request));
        
        var totalUsers = await countHandler.Handler(cancellationToken);

        var userRole = await roleRepository.AllRolesGroupByAsync(cancellationToken);

        var roleStatisticsDto= userRole.ToRoleStatisticsDto(totalUsers);

        return roleStatisticsDto!;
    }
}