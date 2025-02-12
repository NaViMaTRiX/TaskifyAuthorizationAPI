using System.ComponentModel;
using System.Reflection;
using AuthAPI.Application.CQRS.Commands.User;
using AuthAPI.Application.CQRS.Commands.UserAuditLogs;
using AuthAPI.Application.CQRS.Queries.Role;
using AuthAPI.Application.CQRS.Queries.User;
using AuthAPI.Application.CQRS.Queries.UserAuditLogs;
using AuthAPI.Application.Dto;
using AuthAPI.Domain.Enums;
using AuthorizationAPI.Domain.Enums;

namespace AuthAPI.Application.Services.Role;

/// <summary>
/// Сервис управления ролями пользователей
/// </summary>
public class RoleManagementService(
    GetByIdAsyncHandler idAsyncHandler, 
    PutRoleHandler changeRoleHandler, 
    AddUserAuditLog addUserAuditLog,
    GetUsersByRoleAsyncHandler getUsersByRoleHandler,
    GetStatisticsByRoleHandler getStatisticsByRoleHandler,
    GetUserAuditLogsByUser getAuditLogsByUserHandler,
    GetUserRoleStatisticsHandler getUserRoleStatisticsHandler)
{
    /// <summary>
    /// Изменить роль пользователя
    /// </summary>
    public async Task<bool> ChangeUserRoleAsync(
        Guid userId, 
        UserRole newRole, 
        UserRole currentUserRole, 
        CancellationToken cancellationToken = default)
    {
        // Проверка прав на изменение роли
        if (currentUserRole != UserRole.Admin && currentUserRole != UserRole.SuperAdmin)
            throw new UnauthorizedAccessException("Недостаточно прав для изменения роли");

        var user = await idAsyncHandler.Handler(userId, cancellationToken);

        var oldRole = await changeRoleHandler.Handler(user, newRole, cancellationToken);
        
        // Логируем изменение роли с подробным описанием
        await addUserAuditLog.Handler(userId, oldRole, newRole, cancellationToken);

        return true;
    }

    /// <summary>
    /// Получить пользователей с определенной ролью
    /// </summary>
    public async Task<List<Domain.Models.User>> GetUsersByRoleAsync(
        UserRole role, 
        CancellationToken cancellationToken = default)
    {
        return await getUsersByRoleHandler.Handler(role, cancellationToken);
    }

    /// <summary>
    /// Получить количество пользователей с каждой ролью
    /// </summary>
    public async Task<Dictionary<UserRole, int>> GetUserRoleStatisticsAsync(
        CancellationToken cancellationToken = default)
    {
        return await getUserRoleStatisticsHandler.Handler(cancellationToken);
    }

    /// <summary>
    /// Получить расширенную статистику по ролям
    /// </summary>
    public async Task<List<RoleStatisticsDto>> GetDetailedRoleStatisticsAsync(
        CancellationToken cancellationToken = default)
    {
        var roleStats = await getStatisticsByRoleHandler.Handler(cancellationToken);

        return roleStats;
    }

    /// <summary>
    /// Получить описание роли
    /// </summary>
    public static string GetRoleDescription(UserRole role)
    {
        var fieldInfo = role.GetType().GetField(role.ToString());
        var descriptionAttribute = fieldInfo?.GetCustomAttribute<DescriptionAttribute>();
        return descriptionAttribute?.Description ?? role.ToString();
    }

    /// <summary>
    /// Получить статистику по конкретному пользователю
    /// </summary>
    public async Task<UserStatisticsDto> GetUserStatisticsAsync(
        Guid userId, 
        CancellationToken cancellationToken = default)
    {
        // Получаем пользователя
        var user = await idAsyncHandler.Handler(userId, cancellationToken);

        // Получаем аудит-логи пользователя
        var userLogs = await getAuditLogsByUserHandler.Handler(userId, cancellationToken);

        // Статистика входов
        var loginSuccessLogs = userLogs
            .Count(log => log.Action == AuditAction.LoginSuccess);
        var loginFailedLogs = userLogs
            .Count(log => log.Action == AuditAction.LoginFailed);

        // Статистика изменений роли
        var roleChangeLogs = userLogs
            .Count(log => log.Action == AuditAction.RoleChanged);

        return new UserStatisticsDto
        {
            TotalActions = userLogs.Count,
            SuccessfulLogins = loginSuccessLogs,
            FailedLogins = loginFailedLogs,
            FirstLoginDate = userLogs.MinBy(log => log.Timestamp)?.Timestamp,
            LastLoginDate = userLogs.MaxBy(log => log.Timestamp)?.Timestamp,
            Role = user.Role,
            RoleDescription = GetRoleDescription(user.Role),
            RoleChangeCount = roleChangeLogs,
            RecentActions = userLogs.Select(log => new UserAuditLogDto
            {
                Action = log.Action,
                Timestamp = log.Timestamp,
                Details = log.Details
            }).ToList()
        };
    }
}
