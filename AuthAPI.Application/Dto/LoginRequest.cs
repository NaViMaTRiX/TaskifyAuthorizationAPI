using System.ComponentModel.DataAnnotations;
using AuthAPI.Domain.Models;
using MediatR;

namespace AuthAPI.Application.Dto;

public record LoginRequest : IRequest<User>
{
    [Required(ErrorMessage = "Email обязателен.")]
    [EmailAddress(ErrorMessage = "Некорректный email.")]
    [MaxLength(50, ErrorMessage = "Email не должен превышать 100 символов.")]
    public required string Email { get; init; }
    
    [Required(ErrorMessage = "Пароль обязателен.")]
    [MinLength(8, ErrorMessage = "Пароль должен содержать минимум 8 символов.")]
    [MaxLength(100, ErrorMessage = "Пароль слишком длинный.")]
    [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[\W_])[^\s]{8,64}$", 
        ErrorMessage = "Пароль должен содержать от 8 до 64 символов, минимум 1 заглавную букву, 1 строчную букву, 1 цифру и 1 спецсимвол.")]
    public required string Password { get; init; }
}
