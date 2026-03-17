using BuyAndCook.Application.Abstractions;
using BuyAndCook.Domain.Models;

namespace BuyAndCook.Infrastructure.Persistence
{
    public class SqliteIngredientMappingRepository : IIngredientMappingRepository
    {
        private readonly SqliteConnectionFactory _connectionFactory;

        public SqliteIngredientMappingRepository(SqliteConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
            EnsureSchema();
        }

        public Task SaveMappingAsync(IngredientMapping mapping)
        {
            using var connection = _connectionFactory.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                DELETE FROM ingredient_mapping
                WHERE provider_id = $providerId AND ingredient_name = $ingredientName;

                INSERT INTO ingredient_mapping (provider_id, ingredient_name, product_name)
                VALUES ($providerId, $ingredientName, $productName)";
            command.Parameters.AddWithValue("$providerId", mapping.ProviderId);
            command.Parameters.AddWithValue("$ingredientName", mapping.IngredientName);
            command.Parameters.AddWithValue("$productName", mapping.ProductName);
            command.ExecuteNonQuery();
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<IngredientMapping>> GetMappingsAsync(string providerId)
        {
            var results = new List<IngredientMapping>();
            using var connection = _connectionFactory.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT provider_id, ingredient_name, product_name
                FROM ingredient_mapping
                WHERE provider_id = $providerId";
            command.Parameters.AddWithValue("$providerId", providerId);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                results.Add(new IngredientMapping
                {
                    ProviderId = reader.GetString(0),
                    IngredientName = reader.GetString(1),
                    ProductName = reader.GetString(2)
                });
            }

            return Task.FromResult<IReadOnlyList<IngredientMapping>>(results);
        }

        private void EnsureSchema()
        {
            using var connection = _connectionFactory.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS ingredient_mapping (
                    provider_id TEXT NOT NULL,
                    ingredient_name TEXT NOT NULL,
                    product_name TEXT NOT NULL
                );";
            command.ExecuteNonQuery();
        }
    }
}
