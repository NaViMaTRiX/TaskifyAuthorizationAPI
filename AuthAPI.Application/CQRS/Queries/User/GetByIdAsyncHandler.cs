using AuthAPI.DAL.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthAPI.Application.CQRS.Queries.User;

public class GetByIdAsyncHandler(AuthDbContext context)
{
    /// <summary>
    /// Return user by id
    /// </summary>
    /// <param name="userId">Guid type user id</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns></returns>
    public async Task<Domain.Models.User> Handler(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await context.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null) //TODO: надо сделать Exception отдельный
            throw new KeyNotFoundException("Пользователь не найден");

        return user;
    }
}