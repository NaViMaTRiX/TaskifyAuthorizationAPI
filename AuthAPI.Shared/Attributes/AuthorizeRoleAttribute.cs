using AuthAPI.Domain.Enums;
using AuthorizationAPI.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace AuthAPI.Shared.Attributes;

/// <summary>
/// Атрибут авторизации с проверкой минимальной роли
/// </summary>
public class AuthorizeRoleAttribute : AuthorizeAttribute
{
    /// <summary>
    /// Создает атрибут с требуемой минимальной ролью
    /// </summary>
    public AuthorizeRoleAttribute(UserRole minimumRole)
    {
        Policy = $"MinimumRole{minimumRole}";
    }
}
