using AuthAPI.DAL.Interfaces;
using AuthAPI.Domain.Enums;
using MediatR;

namespace AuthAPI.Application.CQRS.Commands.Role;

public record UpdateRoleCommand(Domain.Models.User User, UserRole NewRole) : IRequest<UserRole>;

public class UpdateRoleUserHandler(IRoleRepository roleRepository) : IRequestHandler<UpdateRoleCommand, UserRole>
{
    /// <summary>
    /// Изменяет роль пользователя.
    /// </summary>
    /// <param name="command"></param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>Старая роль пользователя</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public async Task<UserRole> Handle(UpdateRoleCommand command, CancellationToken cancellationToken = default)
    {
        if(command is null)
            throw new ArgumentNullException(nameof(command));
        
        var (user, newRole) = (command.User, command.NewRole);
        
        if(user is null)
            throw new ArgumentNullException(nameof(command.User));
        
        // Нельзя повысить роль выше SuperAdmin
        if (newRole > UserRole.SuperAdmin)
            throw new ArgumentException("Недопустимая роль");
        
        return await roleRepository.UpdateByUserAsync(command.User, command.NewRole, cancellationToken);
    }
}