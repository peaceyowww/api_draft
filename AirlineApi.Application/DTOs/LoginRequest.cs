namespace AirlineApi.Application.DTOs;

// Equivalent of the fields posted by index.php -> login.php
public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
