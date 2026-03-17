using System.Collections.Generic;

namespace BuyAndCook.Domain.Models
{
    public class Recipe
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string SourceUrl { get; set; } = string.Empty;
        public List<Ingredient> Ingredients { get; set; } = new();
        public List<string> Steps { get; set; } = new();

        public Recipe()
        {
        }

        public Recipe(string name)
        {
            Name = name;
        }

        public Recipe(string name, List<Ingredient> ingredients)
        {
            Name = name;
            Ingredients = ingredients;
        }
    }
}
