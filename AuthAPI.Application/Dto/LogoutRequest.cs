namespace AuthAPI.Application.Dto;

public record LogoutRequest
{
    public required string RefreshToken { get; init; }
    public required bool LogoutFromAllDevices { get; init; }
}
