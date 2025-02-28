using System.Linq.Expressions;
using AuthAPI.Application.CQRS.Commands.UserAuditLogs;
using AuthAPI.Application.Interface;
using AuthAPI.DAL.Data;
using AuthAPI.Domain.Enums;
using AuthAPI.Domain.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AuthAPI.Application.Services;

/// <summary>
/// Сервис аудита действий пользователей
/// </summary>
public class AuditService(AuthDbContext context, IMediator mediator) : IAuditService
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
        var action = success ? AuditAction.LoginSuccess : AuditAction.LoginFailed;
        await mediator.Send(new CreateAuditLogCommand(userId, action, ipAddress!), cancellationToken);
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
        var action = allDevices ? AuditAction.LogoutAllDevices : AuditAction.Logout;
        await mediator.Send(new CreateAuditLogCommand(userId, action, ipAddress!), cancellationToken);
    }

    /// <summary>
    /// Записать событие изменения роли
    /// </summary>
    public async Task LogRoleChangeAsync(
        Guid adminId, 
        Guid targetUserId, 
        UserRole oldRole, 
        UserRole newRole,
        string ipAddress,
        CancellationToken cancellationToken = default)
    {
        await mediator.Send(new CreateAuditLogCommand(adminId, AuditAction.RoleChanged, ipAddress), cancellationToken);
    }

    /// <summary>
    /// Возвращает историю действий пользователя
    /// </summary>
    public async Task<(List<UserAuditLog> Logs, int TotalCount)> GetUserAuditLogsAsync(
    Guid userId,
    DateTime? startDate = null,
    DateTime? endDate = null,
    int? pageSize = null,
    int? pageNumber = null,
    string sortField = "Timestamp",
    bool descending = true,
    CancellationToken cancellationToken = default)
    {
        // Базовый запрос с фильтрацией
        var query = context.UserAuditLogs
            .AsNoTracking() // ⚡ Ускоряет запрос, убирая отслеживание изменений
            .Where(log => log.UserId == userId);
        
        // Применяем фильтр по дате, если указан
        if (startDate.HasValue)
            query = query.Where(log => log.Timestamp >= startDate.Value);
        
        if (endDate.HasValue)
            query = query.Where(log => log.Timestamp <= endDate.Value);
        
        // Получаем общее количество записей для пагинации
        int totalCount = await query.CountAsync(cancellationToken);
        
        // Применяем сортировку динамически
        query = ApplySorting(query, sortField, descending);
        
        // Применяем пагинацию, если она указана
        if (pageSize.HasValue && pageNumber.HasValue && pageSize.Value > 0)
        {
            int skip = (pageNumber.Value - 1) * pageSize.Value;
            query = query!.Skip(skip).Take(pageSize.Value);
        }
        
        // Выполняем запрос и возвращаем результаты
        var logs = await query!.ToListAsync(cancellationToken);
        
        return (Logs: logs, TotalCount: totalCount);
    }

    // Метод для динамической сортировки с учетом только существующих полей
    private IQueryable<UserAuditLog>? ApplySorting(
        IQueryable<UserAuditLog>? query,
        string sortField, 
        bool descending)
    {
        // Упрощенная версия, использующая только гарантированно существующие поля
        // Проверяем существование свойства для сортировки через рефлексию
        var propertyInfo = typeof(UserAuditLog).GetProperty(sortField, 
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        
        // Если свойство не найдено или не указано, используем Timestamp по умолчанию
        if (propertyInfo == null || string.IsNullOrEmpty(sortField))
        {
            return descending 
                ? query!.OrderByDescending(log => log.Timestamp)
                : query!.OrderBy(log => log.Timestamp);
        }
        
        // Используем Expression для динамической сортировки по любому полю
        var parameter = Expression.Parameter(typeof(UserAuditLog), "log");
        var property = Expression.Property(parameter, sortField);
        var lambda = Expression.Lambda(property, parameter);
        
        var methodName = descending ? "OrderByDescending" : "OrderBy";
        var method = typeof(Queryable)
            .GetMethods()
            .Single(m => m.Name == methodName && m.IsGenericMethodDefinition && m.GetParameters().Length == 2);
        
        var genericMethod = method.MakeGenericMethod(typeof(UserAuditLog), propertyInfo.PropertyType);
        
        return (IQueryable<UserAuditLog>)genericMethod.Invoke(null, [query!, lambda])!;
    }
    
    /// <inheritdoc/>
    public async Task LogUserDeletionAsync(
        Guid userId,
        string ipAddress,
        CancellationToken cancellationToken = default)
    {
        await mediator.Send(new CreateAuditLogCommand(userId, AuditAction.UserDeleted, ipAddress), cancellationToken);
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
        var action = success ? AuditAction.RegistrationSuccess : AuditAction.RegistrationFailed;
        await mediator.Send(new CreateAuditLogCommand(userId, action, ipAddress!), cancellationToken);
    }

    /// <summary>
    /// Записать событие неудачного обновления токена
    /// </summary>
    public async Task LogRefreshTokenFailureAsync(
        string refreshToken, 
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        await mediator.Send(new CreateAuditLogCommand(null, AuditAction.RefreshTokenFailed, ipAddress!), cancellationToken);
    }
    
    /// <inheritdoc/>
    public async Task LogUserUpdateAsync(
        Guid userId,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        await mediator.Send(new CreateAuditLogCommand(userId, AuditAction.UserUpdate, ipAddress!), cancellationToken);
    }
}
