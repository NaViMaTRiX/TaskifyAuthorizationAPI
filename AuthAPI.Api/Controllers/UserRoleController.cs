using System.Security.Claims;
using AuthAPI.Application.Services.Role;
using AuthAPI.Domain.Enums;
using AuthAPI.Shared.Attributes;
using AuthorizationAPI.Domain.Enums;
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
        // Получаем текущую роль администратора из токена
        var currentUserRoleClaim = User.FindFirst(ClaimTypes.Role);
        
        if (currentUserRoleClaim == null)//TODO: заменить
            return Unauthorized("Не удалось определить роль текущего пользователя");

        var currentUserRole = Enum.Parse<UserRole>(currentUserRoleClaim.Value);

        try//TODO: заменить
        {
            await roleManagementService.ChangeUserRoleAsync(
                userId, 
                newRole, 
                currentUserRole, 
                cancellationToken);

            return Ok($"Роль пользователя изменена на {newRole}");
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid("Отказано в доступе");
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, "Внутренняя ошибка сервера");
        }
    }

    /// <summary>
    /// Получить статистику по ролям
    /// </summary>
    [HttpGet("statistics")]
    [RoleAccess(UserRole.Admin, UserRole.SuperAdmin)]
    public async Task<IActionResult> GetRoleStatistics(CancellationToken cancellationToken)
    {
        try
        {
            var statistics = await roleManagementService.GetDetailedRoleStatisticsAsync(cancellationToken);
            return Ok(statistics);
        }
        catch (Exception e)
        {
            return BadRequest(new { message = e.Message });
        }
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
        try
        {
            var statistics = await roleManagementService.GetUserStatisticsAsync(
                userId,
                cancellationToken);
            return Ok(statistics);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
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
        try
        {
            var users = await roleManagementService.GetUsersByRoleAsync(role, cancellationToken);
            return Ok(users.Select(u => new
            {
                u.Id,
                u.Email,
                u.FirstName,
                u.LastName,
                Role = u.Role.ToString()
            }));
        }
        catch (Exception e)
        {
            return BadRequest(new { message = e.Message });
        }
    }

    /// <summary>
    /// Получить список всех существующих ролей с их описаниями
    /// </summary>
    [HttpGet("all-roles")]
    public IActionResult GetAllRoles()
    {
        try
        {
            var roles = Enum.GetValues(typeof(UserRole))
                .Cast<UserRole>()
                .Select(role => new
                {
                    Role = role,
                    Description = RoleManagementService.GetRoleDescription(role)
                })
                .ToList();

            return Ok(roles);
        }
        catch (Exception e)
        {
            return BadRequest(new { message = e.Message });
        }
    }
}
