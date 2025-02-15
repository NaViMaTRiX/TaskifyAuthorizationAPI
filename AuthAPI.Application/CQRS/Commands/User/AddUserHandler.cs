using AuthAPI.Application.Dto;
using AuthAPI.Application.Interface;
using AuthAPI.DAL.Data;
using AuthAPI.Domain.Enums;

namespace AuthAPI.Application.CQRS.Commands.User;

public class AddUserHandler(AuthDbContext context, IPasswordService passwordService)
{
    public async Task<Domain.Models.User> Handler(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var newUser = new Domain.Models.User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = passwordService.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            CreatedAt = DateTime.UtcNow,
            Role = UserRole.User // Устанавливаем роль по умолчанию
        };
        
        await context.Users.AddAsync(newUser, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return newUser;
    }
}