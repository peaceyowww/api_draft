using MySqlConnector;

namespace AirlineApi.Infrastructure.Database;

public interface IDbConnectionFactory
{
    MySqlConnection CreateConnection();
}
