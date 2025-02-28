namespace AuthAPI.Application.Dto;

public record TokenResponse
{
    public required Domain.Models.RefreshToken RefreshToken { get; init; }
    public required string JwtToken { get; init; }
    
}
