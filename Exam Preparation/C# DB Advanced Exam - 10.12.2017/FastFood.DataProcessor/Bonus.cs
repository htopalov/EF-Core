using System.Linq;
using FastFood.Data;

namespace FastFood.DataProcessor
{
    public static class Bonus
    {
	    public static string UpdatePrice(FastFoodDbContext context, string itemName, decimal newPrice)
        {
            var item = context.Items.FirstOrDefault(i => i.Name == itemName);
            if (item == null)
            {
                return $"Item {itemName} not found!";
            }

            var oldPrice = item.Price.ToString("F2");

            item.Price = newPrice;

            context.Items.Update(item);
            context.SaveChanges();

            return $"{item.Name} Price updated from ${oldPrice} to ${newPrice:F2}";
        }
    }
}
