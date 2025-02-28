using AuthAPI.Application.CQRS.Commands.User;
using AuthAPI.Application.CQRS.Commands.User.UpdateUser;
using AuthAPI.Application.Dto;
using AuthAPI.Application.Interface;
using AuthAPI.Application.Mapping;
using AuthAPI.Application.Services.Role;
using AuthAPI.DAL.Data;
using AuthAPI.Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MediatR;

namespace AuthAPI.Application.Services.User;

/// <summary>
/// Сервис для работы с пользователями
/// </summary>
public class UserService(
    AuthDbContext context,
    ILogger<UserService> logger,
    IAuditService auditService,
    IPasswordService passwordService,
    IMediator mediator) : IUserService
{
    /// <inheritdoc/>
    public async Task<UserDto?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (!EmailValidationHelper.IsValidEmail(email))
            throw new Exception("Некорректный формат электронной почты");//TODO: заменить
        
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
             
        if (user is null)
            throw new Exception($"Пользователь с почтой {email} не найден");//TODO: заменить

        return user.ToUserDto();
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
                    Role = RoleManagementService.GetEnumDescription(u.Role)
                })
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting all users");
            throw;
        }
    }
    
    /// <inheritdoc/>
    public async Task DeleteUserAsync(DeleteUserCommand command, string ipAddress, CancellationToken cancellationToken = default)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            // Удаляем пользователя
            await mediator.Send(new DeleteUserCommand(command.UserId), cancellationToken);

            // Логируем удаление пользователя
            await auditService.LogUserDeletionAsync(command.UserId, ipAddress, cancellationToken);

            logger.LogInformation("User {UserId} was successfully deleted", command.UserId);

            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            logger.LogError(ex, "Error occurred while deleting user {UserId}", command.UserId);
            throw;
        }
    }
    
    /// <inheritdoc/>
    public async Task<UserDto> UpdateUserAsync(UpdateUserCommand command, string ipAddress, CancellationToken cancellationToken = default)
    {
        var user = await context.Users.FindAsync([command.UserId], cancellationToken);
        if (user is null)
            throw new Exception($"Пользователь с ID {command.UserId} не найден"); //TODO: заменить на специальное исключение

        // Проверяем текущий пароль, если меняем пароль или почту
        if ((command.NewPassword != null || command.Email != null) && command.CurrentPassword == null)
            throw new Exception("Для изменения пароля или почты необходимо указать текущий пароль");

        // Проверяем username на уникальность, если он был изменен
        if (!string.IsNullOrEmpty(command.Username) && command.Username != user.Username)
        {
            var existingUsername = await context.Users
                .AnyAsync(u => u.Username == command.Username && u.Id != command.UserId, cancellationToken);
                
            if (existingUsername)
                throw new Exception($"Пользователь с таким никнеймом уже существует: {command.Username}");
                
            user.Username = command.Username;
        }

        // Проверяем email на уникальность и валидность, если он был изменен
        if (!string.IsNullOrEmpty(command.Email) && command.Email != user.Email)
        {
            // Проверяем текущий пароль
            if (!passwordService.VerifyPassword(command.CurrentPassword!, user.PasswordHash))
                throw new Exception("Неверный текущий пароль");

            if (!EmailValidationHelper.IsValidEmail(command.Email))
                throw new Exception("Некорректный формат электронной почты");

            var existingEmail = await context.Users
                .AnyAsync(u => u.Email == command.Email && u.Id != command.UserId, cancellationToken);
                
            if (existingEmail)
                throw new Exception($"Пользователь с такой почтой уже существует: {command.Email}");
                
            user.Email = command.Email;
        }

        // Обновляем пароль, если он был изменен
        if (!string.IsNullOrEmpty(command.NewPassword))
        {
            if (!PasswordValidationHelper.IsValidPassword(command.NewPassword, logger))
                throw new Exception("Новый пароль не соответствует требованиям безопасности");
            
            // Проверяем текущий пароль
            if (!passwordService.VerifyPassword(command.CurrentPassword!, user.PasswordHash))
                throw new Exception("Неверный текущий пароль");
            
            user.PasswordHash = passwordService.HashPassword(command.NewPassword);
        }

        // Обновляем остальные поля
        if (command.FirstName != null)
            user.FirstName = command.FirstName;
            
        if (command.LastName != null)
            user.LastName = command.LastName;

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await context.SaveChangesAsync(cancellationToken);
            
            // Логируем обновление пользователя
            await auditService.LogUserUpdateAsync(user.Id, ipAddress, cancellationToken);
            
            logger.LogInformation("User {UserId} was successfully updated", user.Id);
            await transaction.CommitAsync(cancellationToken);

            return user.ToUserDto();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            logger.LogError(ex, "Error occurred while updating user {UserId}", user.Id);
            throw;
        }
    }
}
