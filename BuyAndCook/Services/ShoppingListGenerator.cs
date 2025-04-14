using System;
using System.Collections.Generic;
using System.Linq;
using BuyAndCook.Models;

namespace BuyAndCook.Services
{
    public class ShoppingListGenerator
    {
        public List<Ingredient> GenerateShoppingList(List<Recipe> recipes)
        {
            var combinedIngredients = new Dictionary<string, Ingredient>();

            foreach (var recipe in recipes)
            {
                foreach (var ingredient in recipe.Ingredients)
                {
                    var key = $"{ingredient.Name.ToLower()}_{ingredient.Unit.ToLower()}";
                    
                    if (combinedIngredients.ContainsKey(key))
                    {
                        combinedIngredients[key].Quantity += ingredient.Quantity;
                    }
                    else
                    {
                        combinedIngredients[key] = new Ingredient(
                            ingredient.Name,
                            ingredient.Quantity,
                            ingredient.Unit
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