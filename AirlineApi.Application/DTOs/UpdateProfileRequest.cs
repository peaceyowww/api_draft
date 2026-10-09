namespace AirlineApi.Application.DTOs;

public class UpdateProfileRequest
{
    public string Email { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? CurrentPassword { get; set; }
    public string? Password { get; set; }
    public string? ConfirmPassword { get; set; }
}
