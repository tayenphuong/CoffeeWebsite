namespace WebBanNuocMVC.Models.Cart
{
    public class CartItem
    {
        public int DrinkId { get; set; }
        public string DrinkName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string? Image { get; set; }
        public decimal Subtotal => Price * Quantity;
    }
}
