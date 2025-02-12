namespace AuthAPI.Application.Dto;

public class LogoutRequest
{
    public string RefreshToken { get; set; } = string.Empty;
    public bool LogoutFromAllDevices { get; set; }
}
