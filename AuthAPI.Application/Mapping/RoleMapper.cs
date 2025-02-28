using AuthAPI.Application.Dto;
using AuthAPI.Application.Services.Role;
using AuthAPI.Domain.Enums;
using AuthAPI.Domain.Models;

namespace AuthAPI.Application.Mapping;

public static class RoleMapper
{
    public static List<RoleStatisticsDto>? ToRoleStatisticsDto(this List<IGrouping<UserRole,User>>? list, int countUser)
    {
        var roleStatisticsDto = list!
            .Select(g => new RoleStatisticsDto
            {
                Role = g.Key,
                RoleDescription = RoleManagementService.GetEnumDescription(g.Key),
                UserCount = g.Count(),
                Percentage = countUser > 0 
                    ? Math.Round(g.Count() * 100.0 / countUser, 2) 
                    : 0
            })
            .OrderByDescending(r => r.UserCount)
            .ToList();

        return roleStatisticsDto;
    }
}