namespace AuthAPI.Application.Dto;

/// <summary>
/// DTO для claims JWT токена
/// </summary>
public class JwtTokenClaimsDto
{
    public Guid UserId { get; set; }
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Role { get; set; }
}
