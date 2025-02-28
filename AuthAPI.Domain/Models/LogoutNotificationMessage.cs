namespace AuthAPI.Domain.Models;

public readonly record struct LogoutNotificationMessage
{
    public Guid UserId { get; init; }
    public required string Email { get; init; }
    public required string IpAddress { get; init; }
    public required string DeviceInfo { get; init;}
    public DateTime LogoutTime { get; init; }
    public bool LogoutFromAllDevices { get; init; } // такое себе
}
