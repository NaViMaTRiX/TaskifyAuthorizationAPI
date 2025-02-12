using AuthAPI.Application.Services.Role;
using AuthAPI.Domain.Enums;
using AuthAPI.Domain.Models;
using AuthorizationAPI.Domain.Enums;

namespace AuthAPI.Application.Dto;

public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Role { get; set; }

    public UserDto() {}

    public UserDto(User user)
    {
        Id = user.Id;
        Email = user.Email;
        FirstName = user.FirstName;
        LastName = user.LastName;
        Role = RoleManagementService.GetRoleDescription(user.Role);;
    }
}
