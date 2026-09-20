using Microsoft.Extensions.Configuration;
using MySqlConnector;

namespace AirlineApi.Infrastructure.Database;

public class DbConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public DbConnectionFactory(IConfiguration configuration)
    {
        _connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Database connection string is not configured.");
    }

    public MySqlConnection CreateConnection() => new MySqlConnection(_connectionString);
}
