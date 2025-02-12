using AuthAPI.DAL.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthAPI.Application.CQRS.Queries.User;

public class CountAsyncHandler(AuthDbContext context)
{
    /// <summary>
    /// Return count all users
    /// </summary>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns></returns>
    public async Task<int> Handler(CancellationToken cancellationToken = default)
    {
        var totalUsers = await context.Users
            .AsNoTracking()
            .CountAsync(cancellationToken); // Не знаю как проверить
        
        return totalUsers;
    }
}