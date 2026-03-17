using WebBanNuocMVC.DesignPatterns.Strategy;

namespace WebBanNuocMVC.Models.Cart
{
    public class ShoppingCart
    {
        public List<CartItem> Items { get; set; } = new List<CartItem>();

        // Thu?c tính Strategy (M?c ??nh là không gi?m giá)
        public IDiscountStrategy DiscountStrategy { get; set; } = new NoDiscount();

        //public int TotalItems => Items.Sum(i => i.Quantity);
        public decimal TotalAmount => Items.Sum(i => i.Subtotal);

        // Tính toán s? ti?n sau khi áp d?ng Strategy
        public decimal DiscountAmount => DiscountStrategy.CalculateDiscount(TotalAmount);
        public decimal FinalAmount => TotalAmount - DiscountAmount;

        public int TotalItems => Items.Sum(i => i.Quantity);



        public void AddItem(CartItem item)
        {
            var existingItem = Items.FirstOrDefault(i => i.DrinkId == item.DrinkId);
            if (existingItem != null)
            {
                existingItem.Quantity += item.Quantity;
            }
            else
            {
                Items.Add(item);
            }
        }

        public void UpdateQuantity(int drinkId, int quantity)
        {
            var item = Items.FirstOrDefault(i => i.DrinkId == drinkId);
            if (item != null)
            {
                if (quantity <= 0)
                {
                    Items.Remove(item);
                }
                else
                {
                    item.Quantity = quantity;
                }
            }
        }

        public void RemoveItem(int drinkId)
        {
            Items.RemoveAll(i => i.DrinkId == drinkId);
        }

        public void Clear()
        {
            Items.Clear();
        }
    }
}
