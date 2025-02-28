using System.Security.Claims;
using AuthAPI.Domain.Enums;
using AuthAPI.Domain.Models;

namespace AuthAPI.Shared.Helpers;

public static class AccessCheckHelper
{
    public static Task<bool> CheckUserAccess(Guid userId, ClaimsPrincipal user)
    {
        if (user is null) throw new ArgumentNullException(nameof(user));

        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var roleClaim = user.FindFirst(ClaimTypes.Role)?.Value;

        if (string.IsNullOrWhiteSpace(userIdClaim) || string.IsNullOrWhiteSpace(roleClaim))
            return Task.FromResult(false);

        var currentUserId = Guid.Parse(userIdClaim);
        var userRole = Enum.Parse<UserRole>(roleClaim);

        return Task.FromResult(userRole == UserRole.SuperAdmin || userRole == UserRole.Admin || currentUserId == userId);
    }
}