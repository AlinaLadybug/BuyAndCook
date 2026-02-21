using BuyAndCook.Application.Abstractions;
using BuyAndCook.Domain.Models;
using Microsoft.Data.Sqlite;

namespace BuyAndCook.Infrastructure.Persistence
{
    public class SqliteMealPlanRepository : IMealPlanRepository
    {
        private readonly SqliteConnectionFactory _connectionFactory;

        public SqliteMealPlanRepository(SqliteConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
            EnsureSchema();
        }

        public Task<IReadOnlyList<MealPlanItem>> GetMealPlanAsync()
        {
            var items = new List<MealPlanItem>();
            using var connection = _connectionFactory.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT recipe_id, sort_order FROM meal_plan ORDER BY sort_order";
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                items.Add(new MealPlanItem
                {
                    RecipeId = reader.GetString(0),
                    SortOrder = reader.GetInt32(1)
                });
            }

            return Task.FromResult<IReadOnlyList<MealPlanItem>>(items);
        }

        public Task SaveMealPlanAsync(IEnumerable<MealPlanItem> items)
        {
            using var connection = _connectionFactory.CreateConnection();
            using var transaction = connection.BeginTransaction();
            using (var deleteCommand = connection.CreateCommand())
            {
                deleteCommand.CommandText = "DELETE FROM meal_plan";
                deleteCommand.ExecuteNonQuery();
            }

            using var insertCommand = connection.CreateCommand();
            insertCommand.CommandText = "INSERT INTO meal_plan (recipe_id, sort_order) VALUES ($recipeId, $sortOrder)";
            var recipeIdParam = insertCommand.CreateParameter();
            recipeIdParam.ParameterName = "$recipeId";
            insertCommand.Parameters.Add(recipeIdParam);

            var sortOrderParam = insertCommand.CreateParameter();
            sortOrderParam.ParameterName = "$sortOrder";
            insertCommand.Parameters.Add(sortOrderParam);

            foreach (var item in items)
            {
                recipeIdParam.Value = item.RecipeId;
                sortOrderParam.Value = item.SortOrder;
                insertCommand.ExecuteNonQuery();
            }

            transaction.Commit();
            return Task.CompletedTask;
        }

        private void EnsureSchema()
        {
            using var connection = _connectionFactory.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS meal_plan (
                    recipe_id TEXT PRIMARY KEY,
                    sort_order INTEGER NOT NULL
                );";
            command.ExecuteNonQuery();
        }
    }
}
