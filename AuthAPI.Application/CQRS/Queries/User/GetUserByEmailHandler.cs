using AuthAPI.DAL.Interfaces;
using AuthAPI.Shared.Helpers;
using MediatR;

namespace AuthAPI.Application.CQRS.Queries.User;

public record GetByEmailRequest(string Email) : IRequest<Domain.Models.User>;

public class GetUserByEmailHandler(IUserRepository userRepository) : IRequestHandler<GetByEmailRequest, Domain.Models.User>
{
    public async Task<Domain.Models.User> Handle(GetByEmailRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request));
        
        var email = request.Email;
        
        // Валидация email
        if (!EmailValidationHelper.IsValidEmail(email) || string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Некорректный формат email.", nameof(email));

        var user = await userRepository.GetByRefreshTokenAsync(email, cancellationToken);
        
        return user!;
    }

}