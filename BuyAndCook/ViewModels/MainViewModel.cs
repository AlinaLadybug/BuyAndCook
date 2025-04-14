using System.Collections.ObjectModel;
using System.Windows.Input;
using BuyAndCook.Models;
using BuyAndCook.Services;
using Microsoft.Maui.Controls;

namespace BuyAndCook.ViewModels
{
    public class MainViewModel : BindableObject
    {
        private readonly ShoppingListGenerator _shoppingListGenerator;
        private string _shoppingListText;

        public ObservableCollection<Recipe> Recipes { get; }
        public ICommand GenerateShoppingListCommand { get; }

        public string ShoppingListText
        {
            get => _shoppingListText;
            set
            {
                _shoppingListText = value;
                OnPropertyChanged();
            }
        }

        public MainViewModel()
        {
            _shoppingListGenerator = new ShoppingListGenerator();
            Recipes = new ObservableCollection<Recipe>();
            GenerateShoppingListCommand = new Command(GenerateShoppingList);

            // Add some sample data
            AddSampleRecipes();
        }

        private void GenerateShoppingList()
        {
            var shoppingList = _shoppingListGenerator.GenerateShoppingList(Recipes.ToList());
            ShoppingListText = _shoppingListGenerator.FormatShoppingList(shoppingList);
        }

        private void AddSampleRecipes()
        {
            var spaghetti = new Recipe("Spaghetti Bolognese");
            spaghetti.Ingredients.Add(new Ingredient("Spaghetti", 500, "g"));
            spaghetti.Ingredients.Add(new Ingredient("Ground Beef", 400, "g"));
            spaghetti.Ingredients.Add(new Ingredient("Tomato Sauce", 500, "ml"));

            var salad = new Recipe("Greek Salad");
            salad.Ingredients.Add(new Ingredient("Cucumber", 1, "piece"));
            salad.Ingredients.Add(new Ingredient("Tomatoes", 2, "pieces"));
            salad.Ingredients.Add(new Ingredient("Feta Cheese", 200, "g"));

            Recipes.Add(spaghetti);
            Recipes.Add(salad);
        }
    }
} 