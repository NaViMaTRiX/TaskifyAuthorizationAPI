using System.ComponentModel;
using System.Reflection;
using AuthAPI.Application.CQRS.Commands.Role;
using AuthAPI.Application.CQRS.Commands.UserAuditLogs;
using AuthAPI.Application.CQRS.Queries.Role;
using AuthAPI.Application.CQRS.Queries.User;
using AuthAPI.Application.CQRS.Queries.UserAuditLogs;
using AuthAPI.Application.Dto;
using AuthAPI.Application.Mapping;
using AuthAPI.Domain.Enums;
using MediatR;

namespace AuthAPI.Application.Services.Role;

/// <summary>
/// Сервис управления ролями пользователей
/// </summary>
public class RoleManagementService(
    IMediator mediator )
{
    /// <summary>
    /// Изменить роль пользователя
    /// </summary>
    public async Task ChangeUserRoleAsync(
        Guid userId,
        UserRole newRole,
        UserRole currentUserRole,
        string ipAddress,
        CancellationToken cancellationToken = default)
    {
        // Проверка прав на изменение роли
        if (currentUserRole != UserRole.Admin && currentUserRole != UserRole.SuperAdmin)
            throw new UnauthorizedAccessException("Недостаточно прав для изменения роли");

        var user = await mediator.Send(new GetByIdRequest(userId), cancellationToken);
        
        //TODO: Надо подумать над этим куда его деть
        var oldRole = await mediator.Send(new UpdateRoleCommand(user, newRole), cancellationToken);
        
        // Логируем изменение роли
        await mediator.Send(new CreateAuditLogCommand(userId, AuditAction.RoleChanged, ipAddress), cancellationToken);
    }

    /// <summary>
    /// Получить пользователей с определенной ролью
    /// </summary>
    public async Task<List<Domain.Models.User>> GetUsersByRoleAsync(
        UserRole role, 
        CancellationToken cancellationToken = default)
    {
        return await mediator.Send(new GetUsersByRoleRequest(role), cancellationToken);
    }

    /// <summary>
    /// Получить количество пользователей с каждой ролью
    /// </summary>
    public async Task<Dictionary<UserRole, int>> GetUserRoleStatisticsAsync(
        CancellationToken cancellationToken = default)
    {
        return await mediator.Send(new GetUserRoleStatisticsRequest(), cancellationToken);
    }

    /// <summary>
    /// Получить расширенную статистику по ролям
    /// </summary>
    public async Task<List<RoleStatisticsDto>> GetDetailedRoleStatisticsAsync(
        CancellationToken cancellationToken = default)
    {
        var roleStats = await mediator.Send(new GetStatisticsByRoleRequest(), cancellationToken);

        return roleStats;
    }

    /// <summary>
    /// Метод получение опиания роли
    /// </summary>
    public static string GetEnumDescription<TEnum>(TEnum value) where TEnum : Enum
    {
        var fieldInfo = value.GetType().GetField(value.ToString());
        var descriptionAttribute = fieldInfo?.GetCustomAttribute<DescriptionAttribute>();
        return descriptionAttribute?.Description ?? value.ToString();
    }


    /// <summary>
    /// Получить статистику по конкретному пользователю
    /// </summary>
    public async Task<UserStatisticsDto> GetUserStatisticsAsync(
        Guid userId, 
        CancellationToken cancellationToken = default)
    {
        // Получаем пользователя
        var user = await mediator.Send(new GetByIdRequest(userId), cancellationToken);

        // Получаем аудит-логи пользователя
        var userLogs = await mediator.Send(new GetAuditLogsByUserRequest(user.Id), cancellationToken);

        // Статистика входов
        var loginSuccessLogs = userLogs
            .Count(log => log.Action == AuditAction.LoginSuccess);
        var loginFailedLogs = userLogs
            .Count(log => log.Action == AuditAction.LoginFailed);

        // Статистика изменений роли
        var roleChangeLogs = userLogs
            .Count(log => log.Action == AuditAction.RoleChanged);

        return userLogs.ToUserStatisticsDto(user, loginSuccessLogs, loginFailedLogs, roleChangeLogs);
    }
}
