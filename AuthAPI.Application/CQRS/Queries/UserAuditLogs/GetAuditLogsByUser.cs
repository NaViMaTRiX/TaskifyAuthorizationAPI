using AuthAPI.DAL.Data;
using AuthAPI.DAL.Interfaces;
using AuthAPI.Domain.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AuthAPI.Application.CQRS.Queries.UserAuditLogs;

public record GetAuditLogsByUserRequest(Guid UserId) : IRequest<List<UserAuditLog>>;

public class GetAuditLogsByUser(IAuditLogRepository auditLogRepository)
    : IRequestHandler<GetAuditLogsByUserRequest, List<UserAuditLog>>
{
    public async Task<List<UserAuditLog>> Handle(GetAuditLogsByUserRequest request, CancellationToken cancellationToken = default)
    {
        var userId = request.UserId;
        
        var userLogs = await auditLogRepository.GetByUserIdAsync(userId, cancellationToken);
        
        return userLogs;
    }
}