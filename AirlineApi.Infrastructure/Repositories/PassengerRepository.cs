using System.Data;
using MySqlConnector;
using AirlineApi.Application.Interfaces;
using AirlineApi.Domain.Entities;
using AirlineApi.Infrastructure.Database;

namespace AirlineApi.Infrastructure.Repositories;

public class PassengerRepository : IPassengerRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public PassengerRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<int> InsertAsync(Passenger passenger)
    {
        try
        {
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            await using var command = new MySqlCommand("SP_InsertPassenger", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

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

            await using var idCommand = new MySqlCommand(
                "SELECT LAST_INSERT_ID();", connection);

            var result = await idCommand.ExecuteScalarAsync();

            return Convert.ToInt32(result);
        }
        catch (MySqlException e)
        {
            throw new DataException(
                "Error inserting passenger.",
                e);
        }
    }

    public async Task<Passenger?> GetActiveByUsernameAsync(string userName)
    {
        try
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
        catch (MySqlException e)
        {
            throw new DataException(
                "Error retrieving passenger by username.",
                e);
        }
    }

    public async Task<Passenger?> GetByIdAsync(int passengerId)
    {
        try
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
        catch (MySqlException e)
        {
            throw new DataException(
                "Error retrieving passenger.",
                e);
        }
    }

    public async Task<bool> UpdateAsync(Passenger passenger, string passwordOrEmpty)
    {
        try
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
        catch (MySqlException e)
        {
            throw new DataException(
                "Error updating passenger.",
                e);
        }
    }

    public async Task<bool> DeactivateAsync(int passengerId)
    {
        try
        {
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            await using var command = new MySqlCommand(
                "SP_DeactivatePassenger", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@p_passengerId", passengerId);

            var affected = await command.ExecuteNonQueryAsync();

            return affected > 0;
        }
        catch (MySqlException e)
        {
            throw new DataException(
                "Error deactivating passenger.",
                e);
        }
    }

    public async Task<bool> UsernameExistsAsync(string userName)
    {
        try
        {
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            await using var command = new MySqlCommand(
                "SELECT COUNT(*) FROM tbl_passengers WHERE userName = @userName",
                connection);

            command.Parameters.AddWithValue("@userName", userName);

            var count = Convert.ToInt32(
                await command.ExecuteScalarAsync());

            return count > 0;
        }
        catch (MySqlException e)
        {
            throw new DataException(
                "Error checking username.",
                e);
        }
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        try
        {
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            await using var command = new MySqlCommand(
                "SELECT COUNT(*) FROM tbl_passengers WHERE Email = @email",
                connection);

            command.Parameters.AddWithValue("@email", email);

            var count = Convert.ToInt32(
                await command.ExecuteScalarAsync());

            return count > 0;
        }
        catch (MySqlException e)
        {
            throw new DataException(
                "Error checking email.",
                e);
        }
    }

    public async Task<bool> PhoneExistsAsync(string phone)
    {
        try
        {
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            await using var command = new MySqlCommand(
                "SELECT COUNT(*) FROM tbl_passengers WHERE Phone = @phone",
                connection);

            command.Parameters.AddWithValue("@phone", phone);

            var count = Convert.ToInt32(
                await command.ExecuteScalarAsync());

            return count > 0;
        }
        catch (MySqlException e)
        {
            throw new DataException(
                "Error checking phone number.",
                e);
        }
    }

    public async Task<string?> GetStatusByUsernameAsync(string userName)
    {
        try
        {
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            await using var command = new MySqlCommand(
                "SELECT Status FROM tbl_passengers WHERE userName = @userName",
                connection);

            command.Parameters.AddWithValue("@userName", userName);

            var result = await command.ExecuteScalarAsync();

            return result as string;
        }
        catch (MySqlException e)
        {
            throw new DataException(
                "Error retrieving passenger status.",
                e);
        }
    }

    private static Passenger MapReaderToPassenger(MySqlDataReader reader)
    {
        return new Passenger
        {
            PassengerId = reader.GetInt32(reader.GetOrdinal("passengerId")),
            FirstName = reader.GetString(reader.GetOrdinal("firstName")),
            MiddleName = reader.IsDBNull(reader.GetOrdinal("middleName"))
                ? null
                : reader.GetString(reader.GetOrdinal("middleName")),
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