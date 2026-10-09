using AirlineApi.Application.DTOs;

namespace AirlineApi.Application.Services;

public interface IPassengerService
{
    Task<ServiceResult<PassengerDto>> RegisterAsync(RegisterPassengerRequest request);

    Task<ServiceResult<PassengerDto>> LoginAsync(LoginRequest request);

    Task<PassengerDto?> GetProfileAsync(int passengerId);

    Task<ServiceResult<bool>> UpdateProfileAsync(int passengerId, UpdateProfileRequest request);

    Task<bool> DeactivateAsync(int passengerId);

    Task<bool> IsUsernameTakenAsync(string userName);
    Task<bool> IsEmailTakenAsync(string email);
    Task<bool> IsPhoneTakenAsync(string phone);
}
