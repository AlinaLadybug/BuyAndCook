using System;
using System.Collections.Generic;
using System.Linq;
using BuyAndCook.Domain.Models;

namespace BuyAndCook.Application.Services
{
    public class ShoppingListGenerator
    {
        public List<Ingredient> GenerateShoppingList(IEnumerable<Recipe> recipes)
        {
            var combinedIngredients = new Dictionary<string, Ingredient>();

            foreach (var recipe in recipes)
            {
                foreach (var ingredient in recipe.Ingredients)
                {
                    var name = ingredient.Name ?? string.Empty;
                    var unit = ingredient.Unit ?? string.Empty;
                    var key = $"{name.ToLowerInvariant()}_{unit.ToLowerInvariant()}";
                    
                    if (combinedIngredients.ContainsKey(key))
                    {
                        combinedIngredients[key].Quantity += ingredient.Quantity;
                    }
                    else
                    {
                        combinedIngredients[key] = new Ingredient(
                            name,
                            ingredient.Quantity,
                            unit
                        );
                    }
                }
            }

            return combinedIngredients.Values.ToList();
        }

        public string FormatShoppingList(List<Ingredient> ingredients)
        {
            return string.Join("\n", ingredients.Select(i => i.ToString()));
        }
    }
} 
