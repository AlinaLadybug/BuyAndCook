using System.Text.Json;
using BuyAndCook.Application.Abstractions;
using BuyAndCook.Application.Models.Silpo;

namespace BuyAndCook.Infrastructure.Persistence
{
    public class SqliteSilpoSessionRepository : ISilpoSessionRepository
    {
        private const int SessionId = 1;
        private readonly SqliteConnectionFactory _connectionFactory;
        private readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public SqliteSilpoSessionRepository(SqliteConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
            EnsureSchema();
        }

        public Task<SilpoSession?> GetSessionAsync()
        {
            using var connection = _connectionFactory.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT payload
                FROM silpo_session
                WHERE id = $id";
            command.Parameters.AddWithValue("$id", SessionId);

            using var reader = command.ExecuteReader();
            if (!reader.Read())
            {
                return Task.FromResult<SilpoSession?>(null);
            }

            var payload = reader.GetString(0);
            if (string.IsNullOrWhiteSpace(payload))
            {
                return Task.FromResult<SilpoSession?>(null);
            }

            var session = JsonSerializer.Deserialize<SilpoSession>(payload, _serializerOptions);
            return Task.FromResult(session);
        }

        public Task SaveSessionAsync(SilpoSession session)
        {
            var payload = JsonSerializer.Serialize(session, _serializerOptions);
            using var connection = _connectionFactory.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                DELETE FROM silpo_session WHERE id = $id;
                INSERT INTO silpo_session (id, payload) VALUES ($id, $payload);";
            command.Parameters.AddWithValue("$id", SessionId);
            command.Parameters.AddWithValue("$payload", payload);
            command.ExecuteNonQuery();
            return Task.CompletedTask;
        }

        public Task ClearSessionAsync()
        {
            using var connection = _connectionFactory.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM silpo_session WHERE id = $id";
            command.Parameters.AddWithValue("$id", SessionId);
            command.ExecuteNonQuery();
            return Task.CompletedTask;
        }

        private void EnsureSchema()
        {
            using var connection = _connectionFactory.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS silpo_session (
                    id INTEGER PRIMARY KEY,
                    payload TEXT NOT NULL
                );";
            command.ExecuteNonQuery();
        }
    }
}
