using System.ComponentModel.DataAnnotations;
using AuthAPI.Domain.Enums;

namespace AuthAPI.Domain.Models;

public class User
{
    [Key]
    [Required]
    public Guid Id { get; init; }
    
    [Required]
    [EmailAddress]
    [MaxLength(50, ErrorMessage = "Email не должен превышать 100 символов.")]
    public required string Email { get; set; }
    
    [Required]
    public required string PasswordHash { get; set; }
    
    [Required]
    [StringLength(30, MinimumLength = 3)]
    public required string Username { get; set; }
    
    [StringLength(30)]
    public string? FirstName { get; set; }
    
    [StringLength(30)]
    public string? LastName { get; set; }
    
    public DateTime CreatedAt { get; init; }
    public DateTime? LastLoginAt { get; set; }
    
    // Добавляем роль пользователя
    [Required]
    public UserRole Role { get; set; } = UserRole.User;
    
    // Навигационное свойство для refresh токенов
    public List<RefreshToken> RefreshTokens { get; init; } = new();


    public void LastTimeLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }
}
