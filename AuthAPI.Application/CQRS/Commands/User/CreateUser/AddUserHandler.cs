using AuthAPI.Application.Interface;
using AuthAPI.Application.Mapping;
using AuthAPI.DAL.Repository;
using AuthAPI.Shared.Helpers;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthAPI.Application.CQRS.Commands.User.CreateUser;

public class AddUserHandler(UserRepository userRepository, IPasswordService passwordService, ILogger<AddUserHandler> logger) 
    : IRequestHandler<CreateUserCommand, Domain.Models.User>
{
    public async Task<Domain.Models.User> Handle(CreateUserCommand userCommand, CancellationToken cancellationToken = default)
    {
        // Валидация email
        if (!EmailValidationHelper.IsValidEmail(userCommand.Email) || string.IsNullOrWhiteSpace(userCommand.Email))
            throw new ArgumentException("Некорректный формат email.", nameof(userCommand.Email));

        // 🔹 Валидация пароля
        if (!PasswordValidationHelper.IsValidPassword(userCommand.Password, logger))
            throw new ArgumentException("Пароль не соответствует требованиям безопасности", nameof(userCommand.Password));
        
        // 🔹 Хешируем пароль асинхронно
        var passwordHash = await Task.Run(() => passwordService.HashPassword(userCommand.Password), cancellationToken);

        var user = userCommand.ToUser(passwordHash);
        var newUser = await userRepository.CreateAsync(user, cancellationToken);
        
        return newUser!;
    }
}