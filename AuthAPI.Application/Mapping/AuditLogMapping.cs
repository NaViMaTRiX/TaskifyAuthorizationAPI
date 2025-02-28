using AuthAPI.Domain.Enums;
using AuthAPI.Domain.Models;

namespace AuthAPI.Application.Mapping;

public static class AuditLogMapping
{
    public static UserAuditLog CreateAuditLog(this AuditAction action, string details, Guid? userId, string? ipAddress)
    {
        return new UserAuditLog
        {
            UserId = userId,
            Action = action,
            IpAddress = ipAddress,
            Details = $"{details}",
            Timestamp = DateTime.UtcNow,
        };
    }
}