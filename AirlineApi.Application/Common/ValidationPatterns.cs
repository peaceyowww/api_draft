using System.Text.RegularExpressions;

namespace AirlineApi.Application.Common;

// Direct port of the regex constants + helpers from validation.php
public static class ValidationPatterns
{
    public static readonly Regex Name = new(@"^[A-Za-z ]{2,50}$", RegexOptions.Compiled);
    public static readonly Regex Address = new(@"^[A-Za-z0-9 ]{5,100}$", RegexOptions.Compiled);
    public static readonly Regex Email = new(@"^[^\s@]+@[^\s@]+\.[^\s@]+$", RegexOptions.Compiled);
    public static readonly Regex Phone = new(@"^[0-9]{11}$", RegexOptions.Compiled);
    public static readonly Regex Username = new(@"^[A-Za-z0-9]{6,50}$", RegexOptions.Compiled);

    // (?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&]).{8,}
    public static readonly Regex Password = new(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&]).{8,}$",
        RegexOptions.Compiled);

    // Equivalent of PHP's isAtLeast18($birthDate)
    public static bool IsAtLeast18(DateOnly birthDate)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        int age = today.Year - birthDate.Year;
        if (birthDate > today.AddYears(-age)) age--;
        return age >= 18;
    }

    // Equivalent of PHP sanitizeInput(): trim + strip tags.
    // (HTML-encoding for output is handled by the API/JSON serializer + the
    // frontend, not baked into the stored value, so data stays clean for reuse.)
    public static string SanitizeInput(string? value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        var trimmed = value.Trim();
        return Regex.Replace(trimmed, "<.*?>", string.Empty);
    }
}
