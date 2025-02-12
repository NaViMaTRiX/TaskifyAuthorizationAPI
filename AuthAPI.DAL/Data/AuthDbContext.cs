using AuthAPI.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthAPI.DAL.Data;

public class AuthDbContext(DbContextOptions<AuthDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<UserAuditLog> UserAuditLogs { get; set; }
    public DbSet<UserLoginActivity> UserLoginActivities { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();
            
        modelBuilder.Entity<User>()
            .HasMany(u => u.RefreshTokens)
            .WithOne(rt => rt.User)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<UserAuditLog>()
            .HasIndex(log => log.UserId);
        modelBuilder.Entity<UserAuditLog>()
            .HasIndex(log => log.Timestamp);
    }
}
