using AuthAPI.DAL.Data;
using AuthAPI.Domain.Enums;
using AuthAPI.Domain.Models;
using AuthorizationAPI.Domain.Enums;

namespace AuthAPI.Application.CQRS.Commands.UserAuditLogs;

public class AddUserAuditLog(AuthDbContext context)
{
    /// <summary>
    /// Добавление роли
    /// </summary>
    /// <param name="userId">Guid</param>
    /// <param name="oldRole">UserRole</param>
    /// <param name="newRole">UserRole</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns></returns>
    public async Task<UserAuditLog> Handler(Guid userId, UserRole oldRole, UserRole newRole, CancellationToken cancellationToken = default)
    {
        var auditLog = new UserAuditLog
        {
            UserId = userId,
            Action = AuditAction.RoleChanged,
            Timestamp = DateTime.UtcNow,
            Details = $"Роль пользователя изменена с {oldRole} на {newRole}"
        };
        context.UserAuditLogs.Add(auditLog);
        await context.SaveChangesAsync(cancellationToken);
        return auditLog;
    }
}