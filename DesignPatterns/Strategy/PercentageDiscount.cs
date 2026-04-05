namespace WebBanNuocMVC.DesignPatterns.Strategy
{
    //Giảm giá theo phần trăm (Percentage Discount)
    public class PercentageDiscount : IDiscountStrategy
    {
        public decimal Percent { get; set; }

        public PercentageDiscount(decimal percent)
        {
            Percent = percent;
        }

        public PercentageDiscount() { }

        public decimal CalculateDiscount(decimal totalAmount) => totalAmount * (Percent / 100);
        public string GetDescription() => $"Giảm {Percent}%";
    }
}
