using System.ComponentModel.DataAnnotations;
using AuthAPI.Application.CQRS.Commands.RefreshToken;
using AuthAPI.Application.CQRS.Commands.User;
using AuthAPI.Application.CQRS.Queries.RefreshToken;
using AuthAPI.Application.CQRS.Queries.User;
using AuthAPI.Application.Dto;
using AuthAPI.Application.Interface;
using AuthAPI.Application.Services.Role;
using AuthAPI.DAL.Data;
using AuthAPI.Domain.Models;
using AuthAPI.Shared.Exceptions;
using AuthAPI.Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using UnauthorizedAccessException = System.UnauthorizedAccessException;

namespace AuthAPI.Application.Services.Authentication;

/// <summary>
/// Сервис аутентификации пользователей
/// </summary>
public class AuthService(
    AuthDbContext context,
    ITokenService tokenService,
    IPasswordService passwordService,
    IAuditService auditService,
    ILoginActivityService loginActivityService,
    GetUserByEmailHandler getUserByEmailHandler,
    AddUserHandler addUserHandler,
    GetRefreshTokenHandler getRefreshTokenHandler,
    AddRefreshTokenHandler addRefreshTokenHandler,
    DeleteRefreshToken deleteRefreshTokenHandler,
    IMessagePublisher messagePublisher) : IAuthService
{
    public async Task<AuthResponse> LoginAsync(LoginRequest request, string? ipAddress, string userAgent, CancellationToken cancellationToken = default)
    {
        // Вычисляем общие значения один раз
        var ip = ipAddress ?? "Unknown";
        var deviceInfo = DeviceInfoParser.GetDeviceInfo(userAgent);

        // Пытаемся получить пользователя по email
        var user = await getUserByEmailHandler.Handler(request, cancellationToken);

        if (user is null)
        {
            await auditService.LogLoginAsync(Guid.Empty, ip, false, cancellationToken);
            await loginActivityService.RecordFailedLoginAsync(Guid.Empty, ip, deviceInfo, cancellationToken);
            throw new InvalidCredentialsException();
        }

        // Проверка пароля
        if (!passwordService.VerifyPassword(request.Password, user.PasswordHash))
        {
            await auditService.LogLoginAsync(Guid.Empty, ip, false, cancellationToken);
            await loginActivityService.RecordFailedLoginAsync(Guid.Empty, ip, deviceInfo, cancellationToken);
            throw new InvalidCredentialsException();
        }
        
        // TODO: Проверка на возможность входа в систему
        
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            // Обновляем время последнего входа
            user.LastLoginAt = DateTime.UtcNow;
            await context.SaveChangesAsync(cancellationToken);
                    
            // Отправляем уведомление в RabbitMQ
            var notificationMessage = new LoginNotificationMessage
            {
                UserId = user.Id,
                Email = user.Email,
                IpAddress = ipAddress,
                DeviceInfo = deviceInfo,
                LoginTime = DateTime.UtcNow,
            };
            await messagePublisher.PublishAsync("login-notifications", notificationMessage);

            // Логируем успешный вход
            await auditService.LogLoginAsync(user.Id, ip, true, cancellationToken);
            await loginActivityService.RecordSuccessfulLoginAsync(user.Id, ip, deviceInfo, cancellationToken);

            // Фиксируем транзакцию
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception)
        {
            // В случае ошибки откатываем транзакцию и пробрасываем исключение дальше
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }

        return await tokenService.GenerateAuthResponseAsync(user, cancellationToken);
    }

    /// <exception cref="ArgumentException"></exception>
    /// <inheritdoc/>
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, string? ipAddress, CancellationToken cancellationToken = default)
    {
        var ip = ipAddress ?? "Unknown";
        // Валидация email
        var emailValidator = new EmailAddressAttribute();
        if (!emailValidator.IsValid(request.Email))
            throw new ArgumentException("Некорректный формат email.", nameof(request.Email));

        // Проверка сложности пароля
        if (request.Password.Length < 8)
            throw new ArgumentException("Пароль должен содержать минимум 8 символов.", nameof(request.Password));

        AuthResponse response;
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var user = await addUserHandler.Handler(request, cancellationToken);

            // Логирование регистрации
            await auditService.LogRegistrationAsync(user.Id, ip, true, cancellationToken);

            // Фиксируем транзакцию
            await transaction.CommitAsync(cancellationToken);
            
            response = await tokenService.GenerateAuthResponseAsync(user, cancellationToken);

        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
        {
            // PostgreSQL error 23505: уникальный индекс нарушен
            await transaction.RollbackAsync(cancellationToken);
            throw new UserAlreadyExistsException();
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }

        return response;
    }

    /// <inheritdoc />
    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken, string? ipAddress, CancellationToken cancellationToken = default)
    {
        // Проверяем наличие refresh токена в базе данных
        var existingRefreshToken = await getRefreshTokenHandler.Handler(refreshToken, cancellationToken);

        // Если токен не найден или просрочен - логируем и возвращаем ошибку
        if (existingRefreshToken is null || existingRefreshToken.ExpiresAt < DateTime.UtcNow || existingRefreshToken.IsRevoked)
        {
            await auditService.LogRefreshTokenFailureAsync(refreshToken, ipAddress, cancellationToken);
            throw new SecurityTokenException();
        }

        var user = existingRefreshToken.User;

        TokenResponse tokens;
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            // Деактивируем старый refresh токен
            existingRefreshToken.IsRevoked = true;
            existingRefreshToken.RevokedAt = DateTime.UtcNow;

            // Удаляем старые токены пользователя
            await deleteRefreshTokenHandler.Handler(user, cancellationToken);

            tokens = await addRefreshTokenHandler.Handler(user, cancellationToken);

            // Фиксируем транзакцию
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }

        // Возвращаем пользователя с новыми токенами
        return new AuthResponse
        {
            Token = tokens.JwtToken,
            RefreshToken = tokens.RefreshToken.ToString(),
            ExpiresAt = DateTime.UtcNow.AddDays(3),
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = RoleManagementService.GetRoleDescription(user.Role)
            }
        };
    }

    public async Task LogoutAsync(LogoutRequest request, string? ipAddress, string userAgent, CancellationToken cancellationToken = default)
    {
        // Вычисляем данные один раз
        var ip = ipAddress ?? "Unknown";
        var deviceInfo = DeviceInfoParser.GetDeviceInfo(userAgent);

        // Проверяем наличие токена
        var token = await context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken, cancellationToken);

        if (token is null)
            throw new InvalidOperationException();

        if (token.ExpiresAt < DateTime.UtcNow)
            throw new UnauthorizedAccessException();

        // Начинаем транзакцию, так как вносим изменения
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            if (request.LogoutFromAllDevices)
            {
                // Отзываем все активные токены пользователя
                var allUserTokens = await context.RefreshTokens
                    .Where(rt => rt.UserId == token.UserId && !rt.IsRevoked) // Фикс условия
                    .ToListAsync(cancellationToken);

                foreach (var userToken in allUserTokens)
                {
                    userToken.IsRevoked = true;
                    userToken.RevokedAt = DateTime.UtcNow;
                }
            }
            else
            {
                // Отзываем только текущий токен
                token.IsRevoked = true;
                token.RevokedAt = DateTime.UtcNow;
            }
            await context.SaveChangesAsync(cancellationToken);
            
            // Отправка сообщения о выходе в очередь
            var notificationMessage = new LogoutNotificationMessage
            {
                UserId = token.UserId,
                Email = token.User.Email,
                IpAddress = ip,
                DeviceInfo = deviceInfo,
                LogoutTime = DateTime.UtcNow
            };
            await messagePublisher.PublishAsync("logout-notifications", notificationMessage);
            await auditService.LogLogoutAsync(token.UserId, ip, request.LogoutFromAllDevices, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

}