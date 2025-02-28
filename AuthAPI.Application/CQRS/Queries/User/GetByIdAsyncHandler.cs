using AuthAPI.DAL.Repository;
using MediatR;

namespace AuthAPI.Application.CQRS.Queries.User;

public record GetByIdRequest(Guid UserId) : IRequest<Domain.Models.User>;

public class GetByIdAsyncHandler(UserRepository userRepository) : IRequestHandler<GetByIdRequest, Domain.Models.User>
{
    /// <summary>
    /// Return user by id
    /// </summary>
    /// <param name="userId">Guid type user id</param>
    /// <param name="request"></param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns></returns>
    public async Task<Domain.Models.User> Handle(GetByIdRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request));
        
        var userId = request.UserId;
        var user = await userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null)
            throw new KeyNotFoundException("Пользователь не найден");

        return user;
    }
}