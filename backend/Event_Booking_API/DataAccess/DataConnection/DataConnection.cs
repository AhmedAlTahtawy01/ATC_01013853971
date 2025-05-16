using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace DataAccess.DataConnection
{
    public class ConnectionSettings
    {
        private readonly string _connectionString;
        private readonly ILogger<ConnectionSettings> _logger;

        public ConnectionSettings(IConfiguration configuration, ILogger<ConnectionSettings> logger)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public MySqlConnection GetConnection() => new MySqlConnection(_connectionString);

        public MySqlCommand GetCommand(string query, MySqlConnection conn)
        {
            var command = new MySqlCommand(query, conn)
            {
                CommandType = CommandType.Text
            };
            return command;
        }

        public async Task<T> ExecuteQueryAsync<T>(string query, Func<DbCommand, Task<T>> func, params MySqlParameter[] parameters)
        {
            await using var conn = GetConnection();
            await using var cmd = GetCommand(query, conn);

            if (parameters != null && parameters.Length > 0)
                cmd.Parameters.AddRange(parameters);

            try
            {
                await conn.OpenAsync().ConfigureAwait(false);
                return await func(cmd).ConfigureAwait(false);
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, $"Database error occurred while executing query.");
                throw new Exception("Database error occurred.", ex);
            }
        }
    }
}
