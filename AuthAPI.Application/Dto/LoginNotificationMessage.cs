namespace AuthAPI.Application.Dto;

public class LoginNotificationMessage
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string DeviceInfo { get; set; } = string.Empty;
    public DateTime LoginTime { get; set; }
}
