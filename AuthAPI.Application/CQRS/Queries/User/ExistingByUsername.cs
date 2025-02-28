using AuthAPI.DAL.Repository;
using MediatR;

namespace AuthAPI.Application.CQRS.Queries.User;

public record ExistingByUsernameRequest(string UserName) : IRequest<bool>;

public class ExistingByUsername(UserRepository userRepository) : IRequestHandler<ExistingByUsernameRequest, bool>
{
    public async Task<bool> Handle(ExistingByUsernameRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request));
        
        var userName = request.UserName;
        // Если username не указан, используем часть email до @
        var username = userName;
        if (string.IsNullOrEmpty(username))
        {
            username = userName.Split('@')[0];
            
            // Проверяем, что username соответствует ограничениям по длине
            if (username.Length < 3)
                username = username.PadRight(3, '1');
            else if (username.Length > 50)
                username = username[..50];
        }
        
        var isExists = await userRepository.ExistsByUsernameAsync(username, cancellationToken);
        
        if (isExists)
            throw new Exception($"Пользователь с таким никнеймом уже существует: {username}");

        return true;
    }
}