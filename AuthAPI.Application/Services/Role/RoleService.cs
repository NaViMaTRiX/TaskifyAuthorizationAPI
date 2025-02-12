using System.ComponentModel;
using System.Reflection;
using AuthAPI.Domain.Enums;
using AuthorizationAPI.Domain.Enums;

namespace AuthAPI.Application.Services.Role;

/// <summary>
/// Сервис для работы с ролями пользователей
/// </summary>
public static class RoleService
{
    /// <summary>
    /// Получить описание роли
    /// </summary>
    public static string GetRoleDescription(UserRole role)
    {
        var fieldInfo = role.GetType().GetField(role.ToString());
        var descriptionAttribute = fieldInfo?.GetCustomAttribute<DescriptionAttribute>();
        return descriptionAttribute?.Description ?? role.ToString();
    }

    /// <summary>
    /// Проверка прав доступа
    /// </summary>
    public static bool HasPermission(UserRole userRole, UserRole requiredRole)
    {
        return (int)userRole >= (int)requiredRole;
    }

    /// <summary>
    /// Получить все доступные роли
    /// </summary>
    public static IEnumerable<(UserRole Role, string Description)> GetAvailableRoles()
    {
        return Enum.GetValues(typeof(UserRole))
            .Cast<UserRole>()
            .Select(role => (role, GetRoleDescription(role)));
    }
}
