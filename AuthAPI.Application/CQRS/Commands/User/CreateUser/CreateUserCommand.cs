using System.ComponentModel.DataAnnotations;
using MediatR;

namespace AuthAPI.Application.CQRS.Commands.User.CreateUser;

public record CreateUserCommand : IRequest<Domain.Models.User>
{
    [Required(ErrorMessage = "Email обязателен.")]
    [EmailAddress(ErrorMessage = "Некорректный email.")]
    [MaxLength(50, ErrorMessage = "Email не должен превышать 100 символов.")]
    public required string Email { get; init; }
    
    [Required(ErrorMessage = "Пароль обязателен.")]
    [MinLength(8, ErrorMessage = "Пароль должен содержать минимум 8 символов.")]
    [MaxLength(30, ErrorMessage = "Пароль слишком длинный.")]
    [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[\W_])[^\s]{8,64}$", 
        ErrorMessage = "Пароль должен содержать от 8 до 64 символов, минимум 1 заглавную букву, 1 строчную букву, 1 цифру и 1 спецсимвол.")]
    public required string Password { get; init; }
    
    [Required]
    [MinLength(3, ErrorMessage = "Никнейм должен содержать минимум 3 символа.")]
    [MaxLength(20, ErrorMessage = "Никнейм не должен превышать 20 символов.")]
    [RegularExpression("^[a-zA-Z0-9.]+$", ErrorMessage = "Никнейм может содержать только буквы, цифры, _ и .")]
    public required string Username { get; init; }
    
    [MinLength(2, ErrorMessage = "Имя слишком короткое.")]
    [MaxLength(20, ErrorMessage = "Имя слишком длинное.")]
    [RegularExpression(@"^[a-zA-Zа-яА-ЯёЁ\s\-]+$", ErrorMessage = "Имя может содержать только буквы, пробел и дефис.")]
    public string? FirstName { get; init; }
    

    [MinLength(2, ErrorMessage = "Фамилия слишком короткое.")]
    [MaxLength(20, ErrorMessage = "Фамилия слишком длинное.")]
    [RegularExpression(@"^[a-zA-Zа-яА-ЯёЁ\s\-]+$", ErrorMessage = "Фамилия может содержать только буквы, пробел и дефис.")]
    public string? LastName { get; init; }
}
