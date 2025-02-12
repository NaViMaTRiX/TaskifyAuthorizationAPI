using AuthAPI.Application.Dto;
using AuthAPI.Application.Interface;
using AuthAPI.Application.Services.Role;
using AuthAPI.DAL.Data;
using AuthorizationAPI.Utils;
using Microsoft.EntityFrameworkCore;

namespace AuthAPI.Application.Services.User;

/// <summary>
/// Сервис для работы с пользователями
/// </summary>
public class UserService(AuthDbContext context) : IUserService
{
    /// <inheritdoc/>
    public async Task<UserDto?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (!EmailValidationHelper.IsValidEmail(email))
            throw new Exception("Некорректный формат электронной почты");//TODO: заменить

        try
        {
             var user = await context.Users
                .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
             
             if (user is null)
                 throw new Exception($"Пользователь с почтой {email} не найден");//TODO: заменить
             
             return new UserDto
             {
                 Id = user.Id,
                 Email = user.Email,
                 FirstName = user.FirstName,
                 LastName = user.LastName,
                 Role = RoleManagementService.GetRoleDescription(user.Role)
             };
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<UserDto>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        try//TODO: заменить
        {
            return await context.Users
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Role = RoleManagementService.GetRoleDescription(u.Role)
                })
                .ToListAsync(cancellationToken);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}
