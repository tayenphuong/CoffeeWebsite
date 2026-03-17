namespace WebBanNuocMVC.DesignPatterns.Strategy
{
    public class FixedAmountDiscount : IDiscountStrategy
    {
        // Phải có thuộc tính công khai (Public Property) để JSON có thể ghi/đọc
        public decimal Amount { get; set; }

        // Constructor nhận tham số phải trùng tên với Property (không phân biệt hoa thường)
        public FixedAmountDiscount(decimal amount)
        {
            Amount = amount;
        }

        // Constructor không tham số (Bắt buộc để JSON Deserialize)
        public FixedAmountDiscount() { }

        public decimal CalculateDiscount(decimal totalAmount)
     => Math.Min(Amount, totalAmount);
        public string GetDescription() => $"Giảm {Amount:N0}đ";
    }
}
