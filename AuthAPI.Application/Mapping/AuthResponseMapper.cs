using AuthAPI.Application.Dto;
using AuthAPI.Application.Services.Role;
using AuthAPI.Domain.Models;

namespace AuthAPI.Application.Mapping;

public static class AuthResponseMapper
{
    public static AuthResponse ToAuthResponse(this TokenResponse tokens, User user)
    {
        return new AuthResponse
        {
            Token = tokens.JwtToken,
            RefreshToken = tokens.RefreshToken.ToString(),
            ExpiresAt = DateTime.UtcNow.AddDays(3),
            User = user.ToUserDto()
        };
    }
    
    public static AuthResponse ToWithAuthResponse(this User user, string jwtToken, string refreshTokenString)
    {
        return new AuthResponse
        {
            Token = jwtToken,
            RefreshToken = refreshTokenString,
            ExpiresAt = DateTime.UtcNow.AddDays(3),
            User = user.ToUserDto()
        };
    }
}