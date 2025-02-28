using AuthAPI.DAL.Repository;

namespace AuthAPI.Application.CQRS.Queries.User;

public class CountAsyncHandler(UserRepository userRepository)
{
    /// <summary>
    /// Метод получение количества всех пользователей
    /// </summary>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>Return count all users</returns>
    public async Task<int> Handler(CancellationToken cancellationToken = default)
    {
        var totalUsers = await userRepository.CountAsync(cancellationToken);
        
        return totalUsers;
    }
}