using AuthAPI.Application.CQRS.Commands.RefreshToken;
using AuthAPI.Application.CQRS.Commands.User.CreateUser;
using AuthAPI.Application.CQRS.Queries.RefreshToken;
using AuthAPI.Application.CQRS.Queries.User;
using AuthAPI.Application.Dto;
using AuthAPI.Application.Interface;
using AuthAPI.Application.Mapping;
using AuthAPI.DAL.Data;
using AuthAPI.DAL.Interfaces;
using AuthAPI.Shared.Exceptions;
using AuthAPI.Shared.Helpers;
using MediatR;
using Microsoft.Extensions.Logging;

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
    IMediator mediator,
    IUserRepository userRepository,
    ILogger<AuthService> logger,
    IMessagePublisher messagePublisher) : IAuthService
{
    public async Task<AuthResponse> LoginAsync(LoginRequest request, string? ipAddress, string userAgent, CancellationToken cancellationToken = default)
    {
        var ip = ipAddress ?? "Unknown";
        var deviceInfo = DeviceInfoParser.GetDeviceInfo(userAgent);
        var deviceFingerprint = DeviceHelper.GenerateDeviceFingerprint(deviceInfo);

        var user = await mediator.Send(new GetByEmailRequest(request.Email), cancellationToken);
        
        // 🔹 Проверка возможности входа (подозрительные попытки, блокировки)
        var isLoginAllowed = await loginActivityService.IsLoginAllowedAsync(
            user.Id, ip, deviceFingerprint, cancellationToken);

        if (!isLoginAllowed)
        {
            // Логируем попытку входа при блокировке
            await auditService.LogLoginAsync(user.Id, ip, false, cancellationToken);

            // Не записываем неудачную попытку в активность, если вход уже заблокирован,
            // чтобы избежать "двойного счета"
            throw new AccountTemporarilyLockedException();
        }

        // 🔹 Валидация пароля
        if (!PasswordValidationHelper.IsValidPassword(request.Password, logger))
            throw new ArgumentException("Пароль не соответствует требованиям безопасности", nameof(request.Password));
        
        // Проверка пароля
        if (!passwordService.VerifyPassword(request.Password, user.PasswordHash))
        {
            await auditService.LogLoginAsync(user.Id, ip, false, cancellationToken);
            await loginActivityService.RecordFailedLoginAsync(user.Id, ip, deviceInfo, cancellationToken);
            throw new InvalidCredentialsException();
        }
        
        // 🔹 Проверка на подозрительный вход для возможной двухфакторной аутентификации
        // var isSuspiciousLogin = await loginActivityService.IsSuspiciousLoginAsync(
        //     user.Id, ip, deviceFingerprint, cancellationToken);
        
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            // Обновляем время последнего входа
            await userRepository.UpdateLastTimeAsync(user, cancellationToken);
            
            // Отправляем уведомление в RabbitMQ
            await messagePublisher.PublishAsync("login-notifications", user.ToNotificationMessage(ipAddress!, deviceInfo));

            // Логируем успешный вход
            await auditService.LogLoginAsync(user.Id, ip, true, cancellationToken);
            await loginActivityService.RecordSuccessfulLoginAsync(user.Id, ip, deviceInfo, cancellationToken);

            var response = await tokenService.GenerateAuthResponseAsync(user, cancellationToken);
            
            // Если вход подозрительный, устанавливаем флаг для 2FA
            // if (isSuspiciousLogin)
            // {
            //     response.RequiresTwoFactor = true;
            //     // Здесь можно добавить логику отправки кода подтверждения
            //     // await _twoFactorService.SendVerificationCodeAsync(user.Id, user.Email, cancellationToken);
            // }
            
            await transaction.CommitAsync(cancellationToken);
            return response;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
    
    public async Task<AuthResponse> RegisterAsync(CreateUserCommand userCommand, string? ipAddress, string userAgent, CancellationToken cancellationToken = default)
    {
        var ip = ipAddress ?? "Unknown";
        var deviceInfo = DeviceInfoParser.GetDeviceInfo(userAgent);

        // 🔹 Проверяем, существует ли уже пользователь с таким Email
        await mediator.Send(new ExistingUserRequest(userCommand.Email), cancellationToken);
        
        //Проверка на уникальность username
        await mediator.Send(new ExistingByUsernameRequest(userCommand.Username), cancellationToken);

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var user = await mediator.Send(new CreateUserCommand
            {
                Email = userCommand.Email,
                Password = userCommand.Password,
                Username = userCommand.Username,
                FirstName = userCommand.FirstName,
                LastName = userCommand.LastName,
            }, cancellationToken);

            // Логирование регистрации
            await auditService.LogRegistrationAsync(user.Id, ip, true, cancellationToken);
            var response = await tokenService.GenerateAuthResponseAsync(user, cancellationToken);
            
            // Отправляем уведомление в RabbitMQ
            await messagePublisher.PublishAsync("login-notifications", user.ToNotificationMessage(ipAddress!, deviceInfo));
            await transaction.CommitAsync(cancellationToken);

            return response;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
        
    }

    public async Task LogoutAsync(LogoutRequest request, string? ipAddress, string userAgent, CancellationToken cancellationToken = default)
    {
        // Вычисляем данные один раз
        var ip = ipAddress ?? "Unknown";
        var deviceInfo = DeviceInfoParser.GetDeviceInfo(userAgent);

        // Проверяем наличие токена
        var token = await mediator.Send(new GetRefreshTokenRequest(request.RefreshToken, ip), cancellationToken);

        // Начинаем транзакцию, так как вносим изменения
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await mediator.Send(new RevokeTokensRequest(request, token), cancellationToken);
            
            // Отправка сообщения о выходе в очередь
            await messagePublisher.PublishAsync("logout-notifications",
                token.User!.NotificationMessage(ipAddress!, deviceInfo));
            
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