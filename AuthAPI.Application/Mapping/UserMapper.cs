using AuthAPI.Application.CQRS.Commands.User.CreateUser;
using AuthAPI.Application.Dto;
using AuthAPI.Application.Services.Role;
using AuthAPI.Domain.Enums;
using AuthAPI.Domain.Models;

namespace AuthAPI.Application.Mapping;

public static class UserMapper
{
    public static UserDto ToUserDto(this User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = UserRole.User.ToString()
        };
    }
    
    public static User ToUser(this CreateUserCommand userDto, string passwordHash)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = userDto.Email,
            PasswordHash = passwordHash,
            Username = userDto.Username,
            FirstName = userDto.FirstName,
            LastName = userDto.LastName,
            CreatedAt = DateTime.UtcNow,
            Role = UserRole.User,
        };
    }
    
    public static LoginNotificationMessage ToNotificationMessage(this User user, string ipAddress, string deviceInfo)
    {
        return new LoginNotificationMessage
        {
            UserId = user.Id,
            Email = user.Email,
            IpAddress = ipAddress,
            DeviceInfo = deviceInfo,
            LoginTime = DateTime.UtcNow,
        };
    }
    
    public static LogoutNotificationMessage NotificationMessage(this User user, string ipAddress, string deviceInfo)
    {
        return new LogoutNotificationMessage
        {
            UserId = user.Id,
            Email = user.Email,
            IpAddress = ipAddress,
            DeviceInfo = deviceInfo,
            LogoutTime = DateTime.UtcNow
        };
    }
    
    public static RefreshToken ToRefreshTokenDto(this User user, string refreshToken)
    {
        return new RefreshToken
        {
            Token = refreshToken,
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(14)
        };
    }
    
    public static UserStatisticsDto ToUserStatisticsDto(this List<UserAuditLog>? userLogs, User user,
        int loginSuccessLogs, int loginFailedLogs, int roleChangeLogs)
    {
        return new UserStatisticsDto
        {
            TotalActions = userLogs!.Count,
            SuccessfulLogins = loginSuccessLogs,
            FailedLogins = loginFailedLogs,
            FirstLoginDate = userLogs.MinBy(log => log.Timestamp)?.Timestamp,
            LastLoginDate = userLogs.MaxBy(log => log.Timestamp)?.Timestamp,
            Role = user.Role,
            RoleDescription = RoleManagementService.GetEnumDescription(user.Role),
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
