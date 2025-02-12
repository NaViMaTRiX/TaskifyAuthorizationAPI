namespace AuthAPI.Domain.Models;

public class LogoutNotificationMessage
{
    public Guid UserId { get; set; }
    public string? Email { get; set; }
    public string? IpAddress { get; set; }
    public string? DeviceInfo { get; set;}
    public DateTime LogoutTime { get; set; }
    public bool LogoutFromAllDevices { get; set; } // такое себе
}
