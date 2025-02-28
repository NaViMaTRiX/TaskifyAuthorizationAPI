namespace AuthAPI.Application.Dto;

public readonly record struct LoginNotificationMessage
{
    public Guid UserId { get; init; }
    public required string Email { get; init; }
    public required string IpAddress { get; init; }
    public required string DeviceInfo { get; init; }
    public DateTime LoginTime { get; init; }
}
