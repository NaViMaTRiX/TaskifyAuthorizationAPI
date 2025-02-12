using AuthAPI.Application.Interface;
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
    public async Task<IActionResult> GetUserByEmail(
        [FromQuery] string email, 
        CancellationToken cancellationToken)
    {
        var user = await userService.GetUserByEmailAsync(email, cancellationToken);
        return Ok(user);
    }

    /// <summary>
    /// Получение списка всех пользователей
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllUsers(CancellationToken cancellationToken)
    {
        var users = await userService.GetAllUsersAsync(cancellationToken);
        return Ok(users);
    }
}
