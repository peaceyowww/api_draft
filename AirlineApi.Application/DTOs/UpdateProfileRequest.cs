namespace AirlineApi.Application.DTOs;

// Equivalent of the fields posted by edit_profile.php -> update_profile.php
public class UpdateProfileRequest
{
    public string Email { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;

    // Optional: leave blank to keep the current password (same as the PHP version)
    public string? Password { get; set; }
    public string? ConfirmPassword { get; set; }
}
