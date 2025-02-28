using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AuthAPI.Application.Dto;

namespace AuthAPI.Application.Mapping;

public static class JwtTokenMapping
{
    public static JwtTokenClaimsDto ToJwtTokenClaimsDto(this JwtSecurityToken jwtToken)
    {
        return new JwtTokenClaimsDto
        {
            UserId = Guid.TryParse(jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value, out var id) ? id : Guid.Empty,
            Email = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value,
            FirstName = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value,
            LastName = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname)?.Value,
            Role = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value
        };
    }
}