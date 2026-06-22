using Microsoft.Extensions.Options;
using Npgsql;

namespace PlatformApp.Infrastructure.Persistence;

public sealed class PostgreSqlConnectionFactory
{
    private readonly PersistenceOptions _options;

    public PostgreSqlConnectionFactory(IOptions<PersistenceOptions> options)
    {
        _options = options.Value;
    }

    public NpgsqlConnection CreateConnection()
    {
        return new NpgsqlConnection(_options.ConnectionString);
    }
}
