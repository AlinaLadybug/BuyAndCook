using System.Text.Json;
using BuyAndCook.Application.Abstractions;
using BuyAndCook.Domain.Models;

namespace BuyAndCook.Infrastructure.Services
{
    public class RecipeDataService : IRecipeProvider
    {
        private readonly IRecipeDataSource _dataSource;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private IReadOnlyList<Recipe>? _cache;

        public RecipeDataService(IRecipeDataSource dataSource)
        {
            _dataSource = dataSource;
        }

        public async Task<IReadOnlyList<Recipe>> GetRecipesAsync()
        {
            if (_cache != null)
            {
                return _cache;
            }

            using var stream = await _dataSource.OpenStreamAsync();
            var data = await JsonSerializer.DeserializeAsync<RecipeData>(stream, _jsonOptions);
            _cache = data?.Recipes ?? new List<Recipe>();
            return _cache;
        }
    }
}
