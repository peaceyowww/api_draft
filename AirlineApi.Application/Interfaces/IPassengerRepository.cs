using AirlineApi.Domain.Entities;

namespace AirlineApi.Application.Interfaces;

public interface IPassengerRepository
{
    Task<int> InsertAsync(Passenger passenger);


    Task<Passenger?> GetActiveByUsernameAsync(string userName);

    Task<Passenger?> GetByIdAsync(int passengerId);

    Task<bool> UpdateAsync(Passenger passenger, string passwordOrEmpty);

    Task<bool> DeactivateAsync(int passengerId);

    Task<bool> UsernameExistsAsync(string userName);
    Task<bool> EmailExistsAsync(string email);
    Task<bool> PhoneExistsAsync(string phone);

    Task<string?> GetStatusByUsernameAsync(string userName);

    Task<string?> GetPasswordHashAsync(int passengerId);
}
