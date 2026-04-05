using System.Drawing;

namespace WebBanNuocMVC.Models.Cart
{
    public class CartItem
    {
        public int DrinkId { get; set; }
        public string Size { get; set; } = "S"; // Thêm dòng này
        public string DrinkName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string? Image { get; set; }
        public decimal Subtotal => Price * Quantity;

        // Tạo mã định danh duy nhất cho item trong giỏ (ID + Size)
        public string ItemKey => $"{DrinkId}_{Size}";
    }
}
