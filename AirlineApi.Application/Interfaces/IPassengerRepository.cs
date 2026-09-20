using AirlineApi.Domain.Entities;

namespace AirlineApi.Application.Interfaces;

// Same role as config.php's Config class: the Application layer
// only knows this contract, never MySqlConnector/ADO.NET directly.
public interface IPassengerRepository
{
    // Calls SP_InsertPassenger. Returns the new passengerId.
    Task<int> InsertAsync(Passenger passenger);

    // Calls SP_LoginPassenger (WHERE Status = 'ACTIVE' only), like loginPassenger().
    Task<Passenger?> GetActiveByUsernameAsync(string userName);

    // Calls SP_GetPassenger.
    Task<Passenger?> GetByIdAsync(int passengerId);

    // Calls SP_UpdatePassenger. Pass an empty string for password to keep the current one.
    Task<bool> UpdateAsync(Passenger passenger, string passwordOrEmpty);

    // Calls SP_DeactivatePassenger.
    Task<bool> DeactivateAsync(int passengerId);

    // Plain SELECT COUNT queries, same as usernameExists()/emailExists() in config.php.
    Task<bool> UsernameExistsAsync(string userName);
    Task<bool> EmailExistsAsync(string email);
    Task<bool> PhoneExistsAsync(string phone);

    // Equivalent of getStatusByUsername().
    Task<string?> GetStatusByUsernameAsync(string userName);
}
