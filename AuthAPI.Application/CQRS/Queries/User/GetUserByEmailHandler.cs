using AuthAPI.Application.Dto;
using AuthAPI.DAL.Data;
using AuthAPI.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace AuthAPI.Application.CQRS.Queries.User;

public class GetUserByEmailHandler(AuthDbContext context)
{
    public async Task<Domain.Models.User> Handler(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
        
        return user;
    }

}