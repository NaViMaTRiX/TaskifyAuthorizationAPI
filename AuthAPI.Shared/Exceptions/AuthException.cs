using System.Net;

namespace AuthAPI.Shared.Exceptions;

public abstract class AuthException(HttpStatusCode statusCode, string message) : Exception(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
}

// Ошибки аутентификации (401 Unauthorized)
public class InvalidCredentialsException() : AuthException(HttpStatusCode.Unauthorized, "Неверный логин или пароль");
public class SecurityTokenException() : AuthException(HttpStatusCode.Unauthorized, "Токен недействителен или истёк");

// Ошибки авторизации (403 Forbidden)
public class UnauthorizedAccessException() : AuthException(HttpStatusCode.Forbidden, "У вас недостаточно прав для выполнения данной операции");

// Ошибки регистрации (409 Conflict)
public class UserAlreadyExistsException() : AuthException(HttpStatusCode.Conflict, "Пользователь с таким email уже существует");

// Ошибки безопасности (429 Too Many Requests)
public class TooManyRequestsException() : AuthException(HttpStatusCode.TooManyRequests, "Слишком много неудачных попыток. Попробуйте позже");

// Ошибки сервера (500 Internal Server Error)
public class DatabaseConnectionException() : AuthException(HttpStatusCode.InternalServerError, "Ошибка соединения с базой данных. Попробуйте позже");

public class AccountTemporarilyLockedException() : AuthException(HttpStatusCode.Conflict, "Вход временно заблокирован из-за подозрительной активности");
