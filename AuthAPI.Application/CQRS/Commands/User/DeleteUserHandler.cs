using AuthAPI.DAL.Interfaces;
using MediatR;

namespace AuthAPI.Application.CQRS.Commands.User;

public record DeleteUserCommand(Guid UserId) : IRequest<Unit>;

public class DeleteUserHandler(IUserRepository userRepository) : IRequestHandler<DeleteUserCommand, Unit>
{
    public async Task<Unit> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request));
        
        await userRepository.DeleteAsync(request.UserId, cancellationToken);
        return Unit.Value;
    }
}