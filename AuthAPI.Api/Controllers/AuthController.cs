using AuthAPI.Application.CQRS.Commands.User.CreateUser;
using AuthAPI.Application.Dto;
using AuthAPI.Application.Interface;
using AuthAPI.Application.Services.Authentication;
using AuthAPI.Shared.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthAPI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService, TokenService tokenService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var ipAddress = IpAddressHelper.GetClientIpAddress(HttpContext);
        var userAgent = HttpContext.Request.Headers.UserAgent.ToString();
        var response = await authService.LoginAsync(request, ipAddress, userAgent, cancellationToken);
        return Ok(response);
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(CreateUserCommand userCommand, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var ipAddress = IpAddressHelper.GetClientIpAddress(HttpContext);
        var userAgent = HttpContext.Request.Headers.UserAgent.ToString();
        var response = await authService.RegisterAsync(userCommand, ipAddress, userAgent, cancellationToken);
        return Ok(response);
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<AuthResponse>> RefreshToken([FromBody] string expiredAccessToken, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var ipAddress = IpAddressHelper.GetClientIpAddress(HttpContext);
        var response = await tokenService.RefreshTokenAsync(expiredAccessToken, ipAddress,cancellationToken);
        return Ok(response);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(LogoutRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var ipAddress = IpAddressHelper.GetClientIpAddress(HttpContext);
        var userAgent = HttpContext.Request.Headers.UserAgent.ToString();
        await authService.LogoutAsync(request, ipAddress, userAgent, cancellationToken);
        return Ok();
    }
}
