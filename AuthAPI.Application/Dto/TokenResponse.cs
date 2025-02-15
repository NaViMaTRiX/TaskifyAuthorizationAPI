namespace AuthAPI.Application.Dto;

public record TokenResponse
{
    public Domain.Models.RefreshToken RefreshToken { get; init; }
    public string JwtToken { get; init; }
    
}
