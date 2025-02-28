using System.Security.Claims;
using AuthAPI.Application.Services.Role;
using AuthAPI.Domain.Enums;
using AuthAPI.Shared.Attributes;
using AuthAPI.Shared.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace AuthAPI.Api.Controllers;

/// <summary>
/// Контроллер управления ролями пользователей
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UserRoleController(RoleManagementService roleManagementService) : ControllerBase
{
    /// <summary>
    /// Изменить роль пользователя
    /// </summary>
    [HttpPut("change-role")]
    [RoleAccess(UserRole.Admin, UserRole.SuperAdmin)]
    public async Task<IActionResult> ChangeUserRole(
        [FromQuery] Guid userId, 
        [FromQuery] UserRole newRole,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var ipAddress = IpAddressHelper.GetClientIpAddress(HttpContext);
        // Получаем текущую роль администратора из токена
        var currentUserRoleClaim = User.FindFirst(ClaimTypes.Role);
        
        if (currentUserRoleClaim is null)//TODO: заменить
            return Unauthorized("Не удалось определить роль текущего пользователя");

        var currentUserRole = Enum.Parse<UserRole>(currentUserRoleClaim.Value);

        await roleManagementService.ChangeUserRoleAsync(
            userId, 
            newRole, 
            currentUserRole,
            ipAddress,
            cancellationToken);

        return Ok($"Роль пользователя изменена на {newRole}");
    }

    /// <summary>
    /// Получить статистику по ролям
    /// </summary>
    [HttpGet("statistics")]
    [RoleAccess(UserRole.Admin, UserRole.SuperAdmin)]
    public async Task<IActionResult> GetRoleStatistics(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var statistics = await roleManagementService.GetDetailedRoleStatisticsAsync(cancellationToken);
        return Ok(statistics);
    }

    /// <summary>
    /// Получить статистику по пользователю
    /// </summary>
    [HttpGet("user-statistics")]
    [RoleAccess(UserRole.Admin, UserRole.SuperAdmin)]
    public async Task<IActionResult> GetUserStatistics(
        [FromQuery] Guid userId, 
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var statistics = await roleManagementService.GetUserStatisticsAsync(
            userId,
            cancellationToken);
        return Ok(statistics);
    }

    /// <summary>
    /// Получить пользователей с определенной ролью
    /// </summary>
    [HttpGet("users-by-role")]
    [RoleAccess(UserRole.Admin, UserRole.SuperAdmin)]
    public async Task<IActionResult> GetUsersByRole(
        [FromQuery] UserRole role, 
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var users = await roleManagementService.GetUsersByRoleAsync(role, cancellationToken);
        return Ok(users.Select(u => new
        {
            u.Id,
            u.Email,
            u.Username,
            u.FirstName,
            u.LastName,
            Role = u.Role.ToString()
        })); // TODO: добавить маппинг
    }

    /// <summary>
    /// Получить список всех существующих ролей с их описаниями
    /// </summary>
    [HttpGet("all-roles")]
    public IActionResult GetAllRoles()
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var roles = Enum.GetValues(typeof(UserRole))
            .Cast<UserRole>()
            .Select(role => new
            {
                Role = role,
                Description = RoleManagementService.GetEnumDescription(role)
            })
            .ToList();

        return Ok(roles);

    }
}
