using AuthAPI.DAL.Data;
using AuthAPI.Domain.Enums;

namespace AuthAPI.Application.CQRS.Commands.User;

public class PutRoleHandler(AuthDbContext context)
{
    /// <summary>
    /// Изменяет роль пользователя. Возращает страрую роль пользователя.
    /// </summary>
    /// <param name="user">Type user - пользовать которому надо изменить роль</param>
    /// <param name="newRole">Type UserRole - новая роль пользователя</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>Старая роль пользователя</returns>
    /// <exception cref="ArgumentException"></exception>
    public async Task<UserRole> Handler(Domain.Models.User user, UserRole newRole, CancellationToken cancellationToken = default)
    {
        // Нельзя повысить роль выше SuperAdmin
        if (newRole > UserRole.SuperAdmin)
            throw new ArgumentException("Недопустимая роль");
        
        // Явно указываем изменение роли
        var oldRole = user.Role;
        user.Role = newRole;
        
        await context.SaveChangesAsync(cancellationToken);
        return oldRole;
    }
}