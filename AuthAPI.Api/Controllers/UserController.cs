using System.Security.Claims;
using AuthAPI.Application.CQRS.Commands.User;
using AuthAPI.Application.CQRS.Commands.User.UpdateUser;
using AuthAPI.Application.Dto;
using AuthAPI.Application.Interface;
using AuthAPI.Domain.Enums;
using AuthAPI.Shared.Attributes;
using AuthAPI.Shared.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthAPI.Api.Controllers;

/// <summary>
/// Контроллер для работы с пользователями
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UserController(IUserService userService) : ControllerBase
{
    /// <summary>
    /// Получение пользователя по электронной почте
    /// </summary>
    [HttpGet("by-email")]
    public async Task<IActionResult> GetUserByEmail([FromQuery] string email, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var ipAddress = IpAddressHelper.GetClientIpAddress(HttpContext);

        // Получаем ID текущего пользователя из токена
        var currentUserEmail = User.FindFirstValue(ClaimTypes.Email);
        var userRole = Enum.Parse<UserRole>(User.FindFirstValue(ClaimTypes.Role)!);

        // Проверяем права на просмотр
        if (userRole != UserRole.Admin && userRole != UserRole.SuperAdmin && !string.Equals(currentUserEmail, email, StringComparison.OrdinalIgnoreCase))
            return Forbid(); 

        var user = await userService.GetUserByEmailAsync(email, cancellationToken);
        return Ok(user);
    }

    /// <summary>
    /// Получение списка всех пользователей (только для администраторов)
    /// </summary>
    [HttpGet]
    [RoleAccess(UserRole.Admin, UserRole.SuperAdmin)]
    public async Task<IActionResult> GetAllUsers(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var users = await userService.GetAllUsersAsync(cancellationToken);
        return Ok(users);
    }
    
    /// <summary>
    /// Удаление пользователя
    /// </summary>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <param name="cancellationToken">Токен отмены</param>
    [HttpDelete("{userId}")]
    public async Task<IActionResult> DeleteUser([FromRoute] Guid userId,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var ipAddress = IpAddressHelper.GetClientIpAddress(HttpContext);
        if (!await AccessCheckHelper.CheckUserAccess(userId, User))
            return Forbid();

        var command = new DeleteUserCommand(userId);
        await userService.DeleteUserAsync(command, ipAddress, cancellationToken);
        return NoContent();
    }
    
    /// <summary>
    /// Обновление всех данных пользователя
    /// </summary>
    [HttpPut("{userId:guid}")]
    public async Task<IActionResult> UpdateUser([FromRoute] Guid userId, [FromBody] UpdateUserCommand command,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var ipAddress = IpAddressHelper.GetClientIpAddress(HttpContext);

        if (!await AccessCheckHelper.CheckUserAccess(userId, User))
            return Forbid();

        if (userId != command.UserId)
            return BadRequest("Идентификатор пользователя в маршруте не совпадает с идентификатором в запросе");

        var updatedUser = await userService.UpdateUserAsync(command, ipAddress, cancellationToken);
        return Ok(updatedUser);
    }
    
    /// <summary>
    /// Обновление никнейма пользователя
    /// </summary>
    [HttpPatch("{userId:guid}/username")]
    public async Task<IActionResult> UpdateUsername([FromRoute] Guid userId, [FromQuery] string username,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var ipAddress = IpAddressHelper.GetClientIpAddress(HttpContext);

        if (!await AccessCheckHelper.CheckUserAccess(userId, User))
            return Forbid();

        var command = new UpdateUserBuilder(userId)
            .WithUsername(username)
            .Build();
            
        var updatedUser = await userService.UpdateUserAsync(command, ipAddress, cancellationToken);
        return Ok(updatedUser);
    }

    /// <summary>
    /// Обновление имени и фамилии пользователя
    /// </summary>
    [HttpPatch("{userId:guid}/name")]
    public async Task<IActionResult> UpdateName([FromRoute] Guid userId, [FromQuery] string? firstName, 
        [FromQuery] string? lastName, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var ipAddress = IpAddressHelper.GetClientIpAddress(HttpContext);

        if (!await AccessCheckHelper.CheckUserAccess(userId, User))
            return Forbid();

        var builder = new UpdateUserBuilder(userId);
        if (firstName != null) builder.WithFirstName(firstName);
        if (lastName != null) builder.WithLastName(lastName);
            
        var updatedUser = await userService.UpdateUserAsync(builder.Build(), ipAddress, cancellationToken);
        return Ok(updatedUser);
    }

    /// <summary>
    /// Обновление пароля пользователя
    /// </summary>
    [HttpPatch("{userId:guid}/password")]
    public async Task<IActionResult> UpdatePassword([FromRoute] Guid userId, [FromBody] UpdatePasswordRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var ipAddress = IpAddressHelper.GetClientIpAddress(HttpContext);
        if (!await AccessCheckHelper.CheckUserAccess(userId, User))
            return Forbid();

        var command = new UpdateUserBuilder(userId)
            .WithNewPassword(request.NewPassword, request.CurrentPassword)
            .Build();
            
        var updatedUser = await userService.UpdateUserAsync(command, ipAddress, cancellationToken);
        return Ok(updatedUser);
    }

    /// <summary>
    /// Обновление email пользователя
    /// </summary>
    [HttpPatch("{userId:guid}/email")]
    public async Task<IActionResult> UpdateEmail([FromRoute] Guid userId, [FromBody] UpdateEmailRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var ipAddress = IpAddressHelper.GetClientIpAddress(HttpContext);
        if (!await AccessCheckHelper.CheckUserAccess(userId, User))
            return Forbid();

        var command = new UpdateUserBuilder(userId)
            .WithEmail(request.Email, request.CurrentPassword)
            .Build();
            
        var updatedUser = await userService.UpdateUserAsync(command, ipAddress, cancellationToken);
        return Ok(updatedUser);
    }
}
