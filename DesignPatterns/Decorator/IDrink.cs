using WebBanNuocMVC.Data;

namespace WebBanNuocMVC.DesignPatterns.Decorator
{
    public interface IDrink
    {
        string GetName();
        decimal GetPrice();
    }

    // "Concrete Component" - Đối tượng gốc lấy từ Database
    public class BaseDrink : IDrink
    {
        private readonly Drink _drink;
        public BaseDrink(Drink drink) => _drink = drink;

        public string GetName() => _drink.DrinkName ?? "Unknown";
        public decimal GetPrice() => _drink.Price ?? 0;
    }
}