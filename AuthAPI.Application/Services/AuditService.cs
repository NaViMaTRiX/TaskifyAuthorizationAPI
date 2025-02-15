using AuthAPI.Application.Interface;
using AuthAPI.DAL.Data;
using AuthAPI.Domain.Enums;
using AuthAPI.Domain.Models;
using AuthorizationAPI.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AuthAPI.Application.Services;

/// <summary>
/// Сервис аудита действий пользователей
/// </summary>
public class AuditService(AuthDbContext context) : IAuditService
{
    /// <summary>
    /// Записать событие входа в систему
    /// </summary>
    public async Task LogLoginAsync(
        Guid userId, 
        string? ipAddress, 
        bool success, 
        CancellationToken cancellationToken = default)
    {
        var auditLog = new UserAuditLog
        {
            UserId = userId,
            Action = success ? AuditAction.LoginSuccess : AuditAction.LoginFailed,
            IpAddress = ipAddress,
            Timestamp = DateTime.UtcNow
        };

        context.UserAuditLogs.Add(auditLog);
        await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Записать событие выхода из системы
    /// </summary>
    public async Task LogLogoutAsync(
        Guid userId, 
        string? ipAddress, 
        bool allDevices, 
        CancellationToken cancellationToken = default)
    {
        var auditLog = new UserAuditLog
        {
            UserId = userId,
            Action = allDevices ? AuditAction.LogoutAllDevices : AuditAction.Logout,
            IpAddress = ipAddress,
            Timestamp = DateTime.UtcNow
        };

        context.UserAuditLogs.Add(auditLog);
        await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Записать событие изменения роли
    /// </summary>
    public async Task LogRoleChangeAsync(
        Guid adminId, 
        Guid targetUserId, 
        UserRole oldRole, 
        UserRole newRole, 
        CancellationToken cancellationToken = default)
    {
        var auditLog = new UserAuditLog
        {
            UserId = adminId,
            Action = AuditAction.RoleChanged,
            Details = $"Роль пользователя изменена с {oldRole} на {newRole}",
            Timestamp = DateTime.UtcNow
        };

        context.UserAuditLogs.Add(auditLog);
        await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Возвращает историю действий пользователя
    /// </summary>
    public async Task<List<UserAuditLog>> GetUserAuditLogsAsync(
        Guid userId, 
        DateTime? startDate = null, 
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var userLog = context.UserAuditLogs
            .Where(log => log.UserId == userId);

        if (startDate.HasValue)
            userLog = userLog.Where(log => log.Timestamp >= startDate.Value);

        if (endDate.HasValue)
            userLog = userLog.Where(log => log.Timestamp <= endDate.Value);

        return await userLog
            .OrderByDescending(log => log.Timestamp)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Записать событие регистрации пользователя
    /// </summary>
    public async Task LogRegistrationAsync(
        Guid userId, 
        string? ipAddress, 
        bool success, 
        CancellationToken cancellationToken = default)
    {
        var auditLog = new UserAuditLog
        {
            UserId = userId,
            Action = success ? AuditAction.RegistrationSuccess : AuditAction.RegistrationFailed,
            IpAddress = ipAddress,
            Timestamp = DateTime.UtcNow
        };

        context.UserAuditLogs.Add(auditLog);
        await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Записать событие неудачного обновления токена
    /// </summary>
    public async Task LogRefreshTokenFailureAsync(
        string refreshToken, 
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var auditLog = new UserAuditLog
        {
            Action = AuditAction.RefreshTokenFailed,
            IpAddress = ipAddress,
            Timestamp = DateTime.UtcNow,
            Details = $"Refresh token failure for token: {refreshToken}"
        };

        context.UserAuditLogs.Add(auditLog);
        await context.SaveChangesAsync(cancellationToken);
    }
}
