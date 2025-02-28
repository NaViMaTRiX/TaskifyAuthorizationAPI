using AuthAPI.Application.CQRS.Commands.User;
using AuthAPI.Application.CQRS.Commands.User.UpdateUser;
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
    
    /// <summary>
    /// Удаление пользователя
    /// </summary>
    /// <param name="command">Команда удаления пользователя</param>
    /// <param name="cancellationToken">Токен отмены</param>
    Task DeleteUserAsync(DeleteUserCommand command, string ipAddress, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Обновление данных пользователя
    /// </summary>
    /// <param name="command">Команда обновления пользователя</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Обновленные данные пользователя</returns>
    Task<UserDto> UpdateUserAsync(UpdateUserCommand command, string ipAddress, CancellationToken cancellationToken = default);
}
