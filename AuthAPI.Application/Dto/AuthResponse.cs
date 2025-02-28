namespace AuthAPI.Application.Dto;

public record AuthResponse
{
    public required string Token { get; init; }
    public required string? RefreshToken { get; init; }
    public DateTime ExpiresAt { get; init; }
    public UserDto? User { get; init; }
    public bool RequiresTwoFactor { get; set; }
}
