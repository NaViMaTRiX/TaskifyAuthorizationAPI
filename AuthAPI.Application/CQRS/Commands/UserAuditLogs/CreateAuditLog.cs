using AuthAPI.Application.Mapping;
using AuthAPI.Application.Services.Role;
using AuthAPI.DAL.Interfaces;
using AuthAPI.Domain.Enums;
using AuthAPI.Domain.Models;
using MediatR;

namespace AuthAPI.Application.CQRS.Commands.UserAuditLogs;

public record CreateAuditLogCommand(Guid? UserId, AuditAction Action, string IpAddress) : IRequest<UserAuditLog>;

public class CreateAuditLog(IAuditLogRepository auditLogRepository) : IRequestHandler<CreateAuditLogCommand, UserAuditLog>
{
    /// <summary>
    /// Добавляет роль в бд
    /// </summary>
    /// <param name="command">Guid UserId, UserRole OldRole, UserRole NewRole</param>
    /// <param name="cancellationToken">token</param>
    /// <returns>Возвращает добавленную роль в бд</returns>
    public async Task<UserAuditLog> Handle(CreateAuditLogCommand command, CancellationToken cancellationToken = default)
    {
        if (command is null) 
            throw new ArgumentNullException(nameof(command));
        
        var (userId, action, ipAddress) = (command.UserId, command.Action, command.IpAddress);
        
        var details = RoleManagementService.GetEnumDescription(action);

        var auditLog = action.CreateAuditLog(details, userId, ipAddress);
        
        await auditLogRepository.CreateAsync(auditLog, cancellationToken);
        
        return auditLog;
    }
}