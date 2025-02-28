using System.Text.RegularExpressions;

namespace AuthAPI.Shared.Helpers;

public static class EmailValidationHelper
{
    private static readonly Lazy<Regex> EmailRegex = new(() => new Regex(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$", 
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture
    ));

    public static bool IsValidEmail(string email)
    {
        return !string.IsNullOrWhiteSpace(email) && EmailRegex.Value.IsMatch(email);
    }
}

