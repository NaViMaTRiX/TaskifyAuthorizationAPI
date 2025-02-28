using AuthAPI.Domain.Models;

namespace AuthAPI.DAL.Interfaces;

public interface ILoginActivitiesRepository
{
    // 🔹 Загружаем последние 5 логинов без отслеживания изменений
    Task<List<UserLoginActivity>> GetLastLoginsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<int> GetFailedAttemptsCount(Guid userId, string ipAddress,DateTime cutoffTime, CancellationToken cancellationToken = default);
    Task<UserLoginActivity> CreateActivityAsync(UserLoginActivity loginActivity, CancellationToken cancellationToken = default);
}