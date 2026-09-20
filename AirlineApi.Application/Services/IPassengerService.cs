using AirlineApi.Application.DTOs;

namespace AirlineApi.Application.Services;

public interface IPassengerService
{
    Task<ServiceResult<PassengerDto>> RegisterAsync(RegisterPassengerRequest request);

    // Mirrors verifyUser(): returns success + passenger, or an error reason
    // ("not_found" | "wrong_password" | "inactive").
    Task<ServiceResult<PassengerDto>> LoginAsync(LoginRequest request);

    Task<PassengerDto?> GetProfileAsync(int passengerId);

    Task<ServiceResult<bool>> UpdateProfileAsync(int passengerId, UpdateProfileRequest request);

    Task<bool> DeactivateAsync(int passengerId);

    // Availability checks used by validation.php's onblur AJAX calls.
    Task<bool> IsUsernameTakenAsync(string userName);
    Task<bool> IsEmailTakenAsync(string email);
    Task<bool> IsPhoneTakenAsync(string phone);
}
