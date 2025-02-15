using AuthAPI.Application.Dto;
using AuthAPI.Domain.Enums;
using AuthAPI.Domain.Models;

namespace AuthAPI.Application.Mapping;

public static class UserMapper
{
    public static UserDto ToUserDto(this User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = UserRole.User.ToString()
        };
    }

    public static User ToUser(this UserDto userDto)
    {
        return new User
        {
            Id = userDto.Id,
            Email = userDto.Email,
            FirstName = userDto.FirstName,
            LastName = userDto.LastName,
            CreatedAt = DateTime.UtcNow,
            Role = UserRole.Guest,
        };
    }
}
