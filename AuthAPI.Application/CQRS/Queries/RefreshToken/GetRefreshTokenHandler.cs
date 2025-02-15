using AuthAPI.DAL.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthAPI.Application.CQRS.Queries.RefreshToken;

public class GetRefreshTokenHandler(AuthDbContext context)
{
    public async Task<Domain.Models.RefreshToken> Handler(string refreshToken, CancellationToken cancellationToken = default)
    {
        var getRefreshToken = await context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken, cancellationToken);

        return getRefreshToken;
    }


}