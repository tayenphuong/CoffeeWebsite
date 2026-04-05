namespace WebBanNuocMVC.DesignPatterns.Strategy
{
    public class NoDiscount : IDiscountStrategy
    {
        public NoDiscount() { } // Đảm bảo có constructor mặc định
        public decimal CalculateDiscount(decimal totalAmount) => 0;
        public string GetDescription() => "Không có mã giảm giá";
    }
}
