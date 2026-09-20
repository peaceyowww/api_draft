using System.Data;
using AirlineApi.Application.Interfaces;
using AirlineApi.Domain.Entities;
using AirlineApi.Infrastructure.Database;
using MySqlConnector;

namespace AirlineApi.Infrastructure.Repositories;

// Equivalent of config.php's Config class, but split into Clean
// Architecture's Infrastructure layer and talking through MySqlConnector
// instead of PDO. Calls the exact same stored procedures from airline.sql.
public class PassengerRepository : IPassengerRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public PassengerRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    // Ports Config::insertPassenger() -> CALL SP_InsertPassenger(...)
    public async Task<int> InsertAsync(Passenger passenger)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new MySqlCommand("SP_InsertPassenger", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        // Order must match the stored procedure signature in airline.sql
        command.Parameters.AddWithValue("@p_firstName", passenger.FirstName);
        command.Parameters.AddWithValue("@p_middleName", (object?)passenger.MiddleName ?? DBNull.Value);
        command.Parameters.AddWithValue("@p_lastName", passenger.LastName);
        command.Parameters.AddWithValue("@p_gender", passenger.Gender);
        command.Parameters.AddWithValue("@p_birthDate", passenger.BirthDate.Date);
        command.Parameters.AddWithValue("@p_Email", passenger.Email);
        command.Parameters.AddWithValue("@p_Phone", passenger.Phone);
        command.Parameters.AddWithValue("@p_address", passenger.Address);
        command.Parameters.AddWithValue("@p_userName", passenger.UserName);
        command.Parameters.AddWithValue("@p_password", passenger.Password);
        command.Parameters.AddWithValue("@p_acctType", passenger.AcctType);

        await command.ExecuteNonQueryAsync();

        // SP_InsertPassenger doesn't SELECT anything back, so pull the
        // generated id the same way SCOPE_IDENTITY() is used in SQL Server.
        await using var idCommand = new MySqlCommand("SELECT LAST_INSERT_ID();", connection);
        var result = await idCommand.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    // Ports Config::loginPassenger() -> CALL SP_LoginPassenger(...)
    // (only returns a row when Status = 'ACTIVE', same restriction as the SP)
    public async Task<Passenger?> GetActiveByUsernameAsync(string userName)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new MySqlCommand("SP_LoginPassenger", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.Parameters.AddWithValue("@p_userName", userName);

        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
            return null;

        return new Passenger
        {
            PassengerId = reader.GetInt32(reader.GetOrdinal("passengerId")),
            FirstName = reader.GetString(reader.GetOrdinal("firstName")),
            LastName = reader.GetString(reader.GetOrdinal("lastName")),
            UserName = reader.GetString(reader.GetOrdinal("userName")),
            Password = reader.GetString(reader.GetOrdinal("password")),
            AcctType = reader.GetString(reader.GetOrdinal("acctType")),
            Status = reader.GetString(reader.GetOrdinal("Status"))
        };
    }

    // Ports Config::getPassenger() -> CALL SP_GetPassenger(...)
    public async Task<Passenger?> GetByIdAsync(int passengerId)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new MySqlCommand("SP_GetPassenger", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.Parameters.AddWithValue("@p_passengerId", passengerId);

        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
            return null;

        return MapReaderToPassenger(reader);
    }

    // Ports Config::updatePassenger() -> CALL SP_UpdatePassenger(...)
    // Pass an empty string for passwordOrEmpty to keep the current password,
    // exactly like the "IF p_password = ''" branch in the stored procedure.
    public async Task<bool> UpdateAsync(Passenger passenger, string passwordOrEmpty)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new MySqlCommand("SP_UpdatePassenger", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.AddWithValue("@p_passengerId", passenger.PassengerId);
        command.Parameters.AddWithValue("@p_firstName", passenger.FirstName);
        command.Parameters.AddWithValue("@p_middleName", (object?)passenger.MiddleName ?? DBNull.Value);
        command.Parameters.AddWithValue("@p_lastName", passenger.LastName);
        command.Parameters.AddWithValue("@p_gender", passenger.Gender);
        command.Parameters.AddWithValue("@p_birthDate", passenger.BirthDate.Date);
        command.Parameters.AddWithValue("@p_Email", passenger.Email);
        command.Parameters.AddWithValue("@p_Phone", passenger.Phone);
        command.Parameters.AddWithValue("@p_address", passenger.Address);
        command.Parameters.AddWithValue("@p_password", passwordOrEmpty);

        var affected = await command.ExecuteNonQueryAsync();
        return affected > 0;
    }

    // Ports Config::deactivatePassenger() -> CALL SP_DeactivatePassenger(...)
    public async Task<bool> DeactivateAsync(int passengerId)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new MySqlCommand("SP_DeactivatePassenger", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.Parameters.AddWithValue("@p_passengerId", passengerId);

        var affected = await command.ExecuteNonQueryAsync();
        return affected > 0;
    }

    // Ports Config::usernameExists() -- plain SELECT, same as the PHP version.
    public async Task<bool> UsernameExistsAsync(string userName)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new MySqlCommand(
            "SELECT COUNT(*) FROM tbl_passengers WHERE userName = @userName", connection);
        command.Parameters.AddWithValue("@userName", userName);

        var count = Convert.ToInt32(await command.ExecuteScalarAsync());
        return count > 0;
    }

    // Ports Config::emailExists().
    public async Task<bool> EmailExistsAsync(string email)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new MySqlCommand(
            "SELECT COUNT(*) FROM tbl_passengers WHERE Email = @email", connection);
        command.Parameters.AddWithValue("@email", email);

        var count = Convert.ToInt32(await command.ExecuteScalarAsync());
        return count > 0;
    }

    // Ports Config::searchUserByContact().
    public async Task<bool> PhoneExistsAsync(string phone)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new MySqlCommand(
            "SELECT COUNT(*) FROM tbl_passengers WHERE Phone = @phone", connection);
        command.Parameters.AddWithValue("@phone", phone);

        var count = Convert.ToInt32(await command.ExecuteScalarAsync());
        return count > 0;
    }

    // Ports Config::getStatusByUsername().
    public async Task<string?> GetStatusByUsernameAsync(string userName)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new MySqlCommand(
            "SELECT Status FROM tbl_passengers WHERE userName = @userName", connection);
        command.Parameters.AddWithValue("@userName", userName);

        var result = await command.ExecuteScalarAsync();
        return result as string;
    }

    private static Passenger MapReaderToPassenger(MySqlDataReader reader)
    {
        return new Passenger
        {
            PassengerId = reader.GetInt32(reader.GetOrdinal("passengerId")),
            FirstName = reader.GetString(reader.GetOrdinal("firstName")),
            MiddleName = reader.IsDBNull(reader.GetOrdinal("middleName")) ? null : reader.GetString(reader.GetOrdinal("middleName")),
            LastName = reader.GetString(reader.GetOrdinal("lastName")),
            Gender = reader.GetString(reader.GetOrdinal("gender")),
            BirthDate = reader.GetDateTime(reader.GetOrdinal("birthDate")),
            Email = reader.GetString(reader.GetOrdinal("Email")),
            Phone = reader.GetString(reader.GetOrdinal("Phone")),
            Address = reader.GetString(reader.GetOrdinal("address")),
            UserName = reader.GetString(reader.GetOrdinal("userName")),
            AcctType = reader.GetString(reader.GetOrdinal("acctType")),
            Status = reader.GetString(reader.GetOrdinal("Status")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("createdAt"))
        };
    }
}
