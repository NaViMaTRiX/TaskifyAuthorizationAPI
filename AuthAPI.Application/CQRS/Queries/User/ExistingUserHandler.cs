using AuthAPI.DAL.Data;
using AuthAPI.DAL.Repository;
using AuthAPI.Shared.Exceptions;
using AuthAPI.Shared.Helpers;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthAPI.Application.CQRS.Queries.User;

public record ExistingUserRequest(string Email) : IRequest<bool>;

public class ExistingUserHandler(UserRepository userRepository, ILogger<ExistingUserHandler> logger)
    : IRequestHandler<ExistingUserRequest, bool>
{
    public async Task<bool> Handle(ExistingUserRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request));
        
        var email = request.Email;
        
        // Валидация email
        if (!EmailValidationHelper.IsValidEmail(email) || string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Некорректный формат email.", nameof(email));

        var isExist = await userRepository.ExistsAsync(email, cancellationToken);
        
        if (isExist)
        {
            logger.LogError("Ошибка регистрации: пользователь с email `{email}` уже существует.", email);
            throw new UserAlreadyExistsException();
        }
        
        return isExist;
    }
}