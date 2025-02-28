using AuthAPI.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AuthAPI.Shared.Attributes;

/// <summary>
/// Расширенный атрибут для контроля доступа на основе ролей
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class RoleAccessAttribute : AuthorizeAttribute, IAuthorizationFilter
{
    private readonly UserRole[] _allowedRoles;

    /// <summary>
    /// Создает атрибут с указанием допустимых ролей
    /// </summary>
    public RoleAccessAttribute(params UserRole[] allowedRoles)
    {
        _allowedRoles = allowedRoles;
    }

    /// <summary>
    /// Проверка авторизации
    /// </summary>
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        // Проверяем аутентификацию
        var user = context.HttpContext.User;
        if (!user.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        // Получаем роль пользователя
        var userRoleClaim = user.FindFirst(System.Security.Claims.ClaimTypes.Role);
        if (userRoleClaim == null)
        {
            context.Result = new ForbidResult();
            return;
        }

        // Парсим роль
        if (!Enum.TryParse<UserRole>(userRoleClaim.Value, out var userRole))
        {
            context.Result = new ForbidResult();
            return;
        }

        // Проверяем доступ
        if (!_allowedRoles.Contains(userRole))
        {
            context.Result = new ForbidResult();
        }
    }
}

/// <summary>
/// Расширения для более удобной работы с ролями
/// </summary>
public static class RoleAccessExtensions
{
    /// <summary>
    /// Проверить, имеет ли пользователь доступ к ресурсу
    /// </summary>
    public static bool HasAccess(this System.Security.Claims.ClaimsPrincipal user, params UserRole[] allowedRoles)
    {
        var userRoleClaim = user.FindFirst(System.Security.Claims.ClaimTypes.Role);
        if (userRoleClaim == null)
            return false;

        if (!Enum.TryParse<UserRole>(userRoleClaim.Value, out var userRole))
            return false;

        return allowedRoles.Contains(userRole);
    }
}
