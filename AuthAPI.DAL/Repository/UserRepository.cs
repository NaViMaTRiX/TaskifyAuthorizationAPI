using AuthAPI.DAL.Data;
using AuthAPI.DAL.Interfaces;
using AuthAPI.Domain.Enums;
using AuthAPI.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthAPI.DAL.Repository;

public abstract class UserRepository(AuthDbContext context) : IUserRepository
{
    public async Task<User?> GetByRefreshTokenAsync(string email, CancellationToken cancellationToken = default)
    {
        var getUser = await context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        
        if(getUser is null)
            throw new Exception($"Пользователь с Email {email} не найден"); //TODO: заменить на специальное исключение
        
        return getUser;
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await context.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Email == email, cancellationToken);
        
        if (user is null)
            throw new Exception($"Пользователь с Email {email} не найден"); //TODO: заменить на специальное исключение
        
        return user;
    }

    public async Task<User?> CreateAsync(User user, CancellationToken cancellationToken = default)
    {
        await context.Users.AddAsync(user, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return user;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await context.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        
        if (user is null)
            throw new Exception($"Пользователь с ID {id} не найден"); //TODO: заменить на специальное исключение
        
        return user;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await GetByIdAsync(id, cancellationToken);
        
        context.Users.Remove(user!);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        var totalUsers = await context.Users
            .AsNoTracking()
            .CountAsync(cancellationToken); // Не знаю как проверить
        
        return totalUsers;
    }

    public Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> ExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        return await context.Users.AnyAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<List<User?>> GetByRoleAsync(UserRole role, CancellationToken cancellationToken = default)
    {
        var users = await context.Users
            .AsNoTracking()
            .Where(u => u.Role == role)
            .ToListAsync(cancellationToken);
        
        if (users is null) //TODO: надо сделать Exception отдельный
            throw new KeyNotFoundException();

        return users!;
    }

    public async Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await context.Users.AnyAsync(u => u.Username == username, cancellationToken);
    }

    public async Task UpdateLastTimeAsync(User user, CancellationToken cancellationToken = default)
    {
        user.LastTimeLogin();
        await context.SaveChangesAsync(cancellationToken);
    }
}