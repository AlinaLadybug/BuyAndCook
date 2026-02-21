namespace BuyAndCook.Domain.Models
{
    public class Ingredient
    {
        public string Name { get; set; } = string.Empty;
        public double Quantity { get; set; }
        public string Unit { get; set; } = string.Empty;

        public Ingredient()
        {
        }

        public Ingredient(string name, double quantity, string unit)
        {
            Name = name;
            Quantity = quantity;
            Unit = unit;
        }

        public override string ToString()
        {
            if (string.IsNullOrWhiteSpace(Unit))
            {
                return $"{Quantity} {Name}".Trim();
            }

            return $"{Quantity} {Unit} {Name}".Trim();
        }
    }
}
