namespace WebBanNuocMVC.DesignPatterns.Decorator
{
    public abstract class DrinkDecorator : IDrink
    {
        protected readonly IDrink _innerDrink;
        public DrinkDecorator(IDrink drink) => _innerDrink = drink;

        public virtual string GetName() => _innerDrink.GetName();
        public virtual decimal GetPrice() => _innerDrink.GetPrice();
    }

    // Trang trí Size M (Cộng thêm 5,000đ)
    public class SizeMDecorator : DrinkDecorator
    {
        public SizeMDecorator(IDrink drink) : base(drink) { }
        public override string GetName() => _innerDrink.GetName() + " (Size M)";
        public override decimal GetPrice() => _innerDrink.GetPrice() + 5000;
    }

    // Trang trí Size L (Cộng thêm 10,000đ)
    public class SizeLDecorator : DrinkDecorator
    {
        public SizeLDecorator(IDrink drink) : base(drink) { }
        public override string GetName() => _innerDrink.GetName() + " (Size L)";
        public override decimal GetPrice() => _innerDrink.GetPrice() + 10000;
    }
}