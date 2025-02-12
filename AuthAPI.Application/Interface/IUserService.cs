using AuthAPI.Application.Dto;

namespace AuthAPI.Application.Interface;

/// <summary>
/// Интерфейс сервиса для работы с пользователями
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Получение пользователя по электронной почте
    /// </summary>
    /// <param name="email">Электронная почта пользователя</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Dto пользователя</returns>
    Task<UserDto?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получение списка всех пользователей
    /// </summary>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Список пользователей</returns>
    Task<IEnumerable<UserDto>> GetAllUsersAsync(CancellationToken cancellationToken = default);
}
