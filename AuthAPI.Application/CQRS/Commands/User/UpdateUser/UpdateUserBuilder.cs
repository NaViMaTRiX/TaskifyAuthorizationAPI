using System.ComponentModel.DataAnnotations;

namespace AuthAPI.Application.CQRS.Commands.User.UpdateUser;

/// <summary>
/// Билдер для создания команды обновления пользователя
/// </summary>
public class UpdateUserBuilder(Guid userId)
{
    private string? _username;
    private string? _firstName;
    private string? _lastName;
    private string? _email;
    private string? _currentPassword;
    private string? _newPassword;

    /// <summary>
    /// Установить новый никнейм пользователя
    /// </summary>
    /// <param name="username">Новый никнейм</param>
    public UpdateUserBuilder WithUsername(string username)
    {
        if (string.IsNullOrEmpty(username))
            throw new ArgumentException("Никнейм не может быть пустым", nameof(username));
            
        if (username.Length < 3 || username.Length > 50)
            throw new ArgumentException("Никнейм должен быть от 3 до 50 символов", nameof(username));
            
        _username = username;
        return this;
    }

    /// <summary>
    /// Установить новое имя пользователя
    /// </summary>
    /// <param name="firstName">Новое имя</param>
    public UpdateUserBuilder WithFirstName(string firstName)
    {
        if (firstName?.Length > 50)
            throw new ArgumentException("Имя не может быть длиннее 50 символов", nameof(firstName));
            
        _firstName = firstName;
        return this;
    }

    /// <summary>
    /// Установить новую фамилию пользователя
    /// </summary>
    /// <param name="lastName">Новая фамилия</param>
    public UpdateUserBuilder WithLastName(string lastName)
    {
        if (lastName?.Length > 50)
            throw new ArgumentException("Фамилия не может быть длиннее 50 символов", nameof(lastName));
            
        _lastName = lastName;
        return this;
    }

    /// <summary>
    /// Установить новый email пользователя
    /// </summary>
    /// <param name="email">Новый email</param>
    /// <param name="currentPassword">Текущий пароль для подтверждения</param>
    public UpdateUserBuilder WithEmail(string email, string currentPassword)
    {
        if (string.IsNullOrEmpty(email))
            throw new ArgumentException("Email не может быть пустым", nameof(email));
            
        if (string.IsNullOrEmpty(currentPassword))
            throw new ArgumentException("Для изменения email необходимо указать текущий пароль", nameof(currentPassword));

        var emailAttribute = new EmailAddressAttribute();
        if (!emailAttribute.IsValid(email))
            throw new ArgumentException("Некорректный формат email", nameof(email));
            
        _email = email;
        _currentPassword = currentPassword;
        return this;
    }

    /// <summary>
    /// Установить новый пароль пользователя
    /// </summary>
    /// <param name="newPassword">Новый пароль</param>
    /// <param name="currentPassword">Текущий пароль для подтверждения</param>
    public UpdateUserBuilder WithNewPassword(string newPassword, string currentPassword)
    {
        if (string.IsNullOrEmpty(newPassword))
            throw new ArgumentException("Новый пароль не может быть пустым", nameof(newPassword));
            
        if (string.IsNullOrEmpty(currentPassword))
            throw new ArgumentException("Для изменения пароля необходимо указать текущий пароль", nameof(currentPassword));
            
        if (newPassword.Length < 8)
            throw new ArgumentException("Пароль должен быть не менее 8 символов", nameof(newPassword));
            
        _newPassword = newPassword;
        _currentPassword = currentPassword;
        return this;
    }

    /// <summary>
    /// Создать команду обновления пользователя
    /// </summary>
    public UpdateUserCommand Build() => new()
    {
        UserId = userId,
        Username = _username,
        FirstName = _firstName,
        LastName = _lastName,
        Email = _email,
        CurrentPassword = _currentPassword,
        NewPassword = _newPassword
    };
}
