using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace AirlineApi.Application.Common;

public static class ValidationPatterns
{
    private static readonly TimeSpan Timeout = TimeSpan.FromMilliseconds(200);
    private static Regex Make(string pattern) => new(pattern, RegexOptions.Compiled, Timeout);

    public static readonly Regex Name = Make(@"^[A-Za-z ]{2,50}$");
    public static readonly Regex Address = Make(@"^[A-Za-z0-9 ]{5,100}$");

    public static readonly Regex Email = Make(@"^[A-Za-z0-9._%+\-]{1,64}@[A-Za-z0-9.\-]{1,190}\.[A-Za-z]{2,}$");
    public static readonly Regex Phone = Make(@"^[0-9]{11}$");
    public static readonly Regex Username = Make(@"^[A-Za-z0-9]{6,50}$");
    private static readonly Regex Password = Make(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&]).{8,64}$");
    private static readonly Regex Tags = Make(@"<[^>]*>");
    private static readonly Regex Control = Make(@"\p{Cc}");

    public static bool IsEmailValid(string email) => email.Length <= 100 && Email.IsMatch(email);

    public static bool IsStrongPassword(string password) =>
        Encoding.UTF8.GetByteCount(password) <= 72 && Password.IsMatch(password);

    public static bool TryParseBirthDate(string? value, out DateOnly date) =>
        DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date);

    public static bool IsAtLeast18(DateOnly birthDate)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        int age = today.Year - birthDate.Year;
        if (birthDate > today.AddYears(-age)) age--;
        return age >= 18;
    }

    public static string SanitizeInput(string? value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        var cleaned = Control.Replace(value.Trim(), string.Empty);
        return Tags.Replace(cleaned, string.Empty).Trim();
    }
}
