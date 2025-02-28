using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AuthAPI.Domain.Models;

public class RefreshToken
{
    [Key]
    public Guid Id { get; init; }
    
    [Required]
    [JsonIgnore] 
    public required string Token { get; init; }
    public bool IsRevoked { get; set; }
    public DateTime ExpiresAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? RevokedAt { get; set; }
    
    // Внешний ключ
    public Guid UserId { get; init; }
    
    [ForeignKey(nameof(UserId))]
    public User? User { get; init; }
    
    public void Revoke()
    {
        if (IsRevoked)
            throw new InvalidOperationException("Токен уже отозван.");

        IsRevoked = true;
        RevokedAt = DateTime.UtcNow;
    }
}
