namespace AuthAPI.Application.Dto;

/// <summary>
/// DTO для claims JWT токена
/// </summary>
public record JwtTokenClaimsDto
{
    public Guid UserId { get; init; }
    public required string? Email { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public required string? Role { get; init; }
}
