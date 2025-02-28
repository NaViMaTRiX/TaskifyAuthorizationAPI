using AuthAPI.Application.Services.Role;
using AuthAPI.Domain.Enums;
using AuthAPI.Domain.Models;
using MediatR;

namespace AuthAPI.Application.Dto;

public record UserDto : IRequest<TokenResponse>
{
    public Guid Id { get; init; }
    public string Email { get; init; }
    public string Username { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string Role { get; init; }

    public UserDto(){}
}
