using System.Data;
using MySqlConnector;

namespace Scholarship.Api.Data;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
    Task<IDbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default);
}

public class MySqlDbConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public MySqlDbConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("ScholarshipDb")
            ?? throw new InvalidOperationException("Connection string 'ScholarshipDb' is not configured.");
    }

    public IDbConnection CreateConnection()
    {
        var connection = new MySqlConnection(_connectionString);
        connection.Open();
        return connection;
    }

    public async Task<IDbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default)
    {
        var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}
