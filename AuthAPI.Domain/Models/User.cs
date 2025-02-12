using System.ComponentModel.DataAnnotations;
using AuthAPI.Domain.Enums;
using AuthorizationAPI.Domain.Enums;

namespace AuthAPI.Domain.Models;

public class User
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    
    [Required]
    public string PasswordHash { get; set; }
    
    [Required]
    public string FirstName { get; set; }
    
    [Required]
    public string LastName { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    
    // Добавляем роль пользователя
    [Required]
    public UserRole Role { get; set; } = UserRole.User;
    
    // Навигационное свойство для refresh токенов
    public List<RefreshToken> RefreshTokens { get; set; } = new();
}
