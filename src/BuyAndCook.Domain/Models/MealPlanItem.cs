namespace BuyAndCook.Domain.Models
{
    public class MealPlanItem
    {
        public string RecipeId { get; set; } = string.Empty;
        public int SortOrder { get; set; }
    }
}
