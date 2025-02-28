using System.ComponentModel;
using System.Reflection;
using AuthAPI.Domain.Enums;

namespace AuthAPI.Application.Services.Role;

/// <summary>
/// Сервис для работы с ролями пользователей
/// </summary>
public static class RoleService
{
    private static readonly Lazy<Dictionary<UserRole, string>> RoleDescriptions = new(() =>
        Enum.GetValues(typeof(UserRole))
            .Cast<UserRole>()
            .ToDictionary(role => role, RoleManagementService.GetEnumDescription)
    );

    /// <summary>
    /// Получить описание роли (из кэша)
    /// </summary>
    public static string GetRoleDescription(UserRole role)
    {
        if (!Enum.IsDefined(typeof(UserRole), role))
            throw new ArgumentException($"Недопустимое значение роли: {role}", nameof(role));

        return RoleDescriptions.Value.TryGetValue(role, out var description) ? description : role.ToString();
    }

    /// <summary>
    /// Проверка прав доступа
    /// </summary>
    public static bool HasPermission(UserRole userRole, UserRole requiredRole)
    {
        if (!Enum.IsDefined(typeof(UserRole), userRole) || !Enum.IsDefined(typeof(UserRole), requiredRole))
            throw new ArgumentException("Передано некорректное значение UserRole.");

        return userRole.CompareTo(requiredRole) >= 0;
    }

    /// <summary>
    /// Получить все доступные роли (из кэша)
    /// </summary>
    public static IEnumerable<(UserRole Role, string Description)> GetAvailableRoles()
    {
        return RoleDescriptions.Value.Select(kv => (kv.Key, kv.Value));
    }
}


