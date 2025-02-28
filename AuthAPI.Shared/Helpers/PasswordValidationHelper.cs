using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace AuthAPI.Shared.Helpers;

public static class PasswordValidationHelper
{
    private static readonly Regex PasswordRegex = new(
        @"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[\W_])[^\s]{8,64}$",
        RegexOptions.Compiled | RegexOptions.ExplicitCapture
    );

    public static bool IsValidPassword(string password, ILogger? logger = null)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            logger?.LogError("Ошибка валидации пароля: пароль пустой или null.");
            return false;
        }

        if (PasswordRegex.IsMatch(password)) 
            return true;
        
        logger?.LogError("Ошибка валидации пароля: `{Password}` не соответствует требованиям безопасности.", password);
        return false;
    }
}
