using AuthAPI.DAL.Repository;
using AuthAPI.Domain.Enums;
using MediatR;

namespace AuthAPI.Application.CQRS.Queries.User;

public record GetUsersByRoleRequest(UserRole Role) : IRequest<List<Domain.Models.User>>;

public class GetUsersByRoleAsyncHandler(UserRepository userRepository)
    : IRequestHandler<GetUsersByRoleRequest, List<Domain.Models.User>>
{
    /// <summary>
    /// Метод получения пользователей по роли
    /// </summary>
    /// <param name="request">User role(type UserRole)</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>Return users by role</returns>
    public async Task<List<Domain.Models.User>> Handle(GetUsersByRoleRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request));
        
        var role = request.Role;
        return  (await userRepository.GetByRoleAsync(role, cancellationToken))!;
    }
}