using System.ComponentModel.DataAnnotations;

namespace AuthAPI.Application.CQRS.Commands.User.UpdateUser;

public record UpdateUserCommand
{
    public required Guid UserId { get; init; }
    
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Никнейм должен быть от 3 до 50 символов")]
    public string? Username { get; init; }
    
    [StringLength(50, ErrorMessage = "Имя не может быть длиннее 50 символов")]
    public string? FirstName { get; init; }
    
    [StringLength(50, ErrorMessage = "Фамилия не может быть длиннее 50 символов")]
    public string? LastName { get; init; }
    
    [EmailAddress(ErrorMessage = "Некорректный формат электронной почты")]
    public string? Email { get; init; }
    
    /// <summary>
    /// Текущий пароль пользователя (необходим для изменения пароля или почты)
    /// </summary>
    public string? CurrentPassword { get; init; }
    
    /// <summary>
    /// Новый пароль
    /// </summary>
    [MinLength(8, ErrorMessage = "Пароль должен быть не менее 8 символов")]
    public string? NewPassword { get; init; }
}
