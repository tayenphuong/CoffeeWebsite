using WebBanNuocMVC.DesignPatterns.Strategy;

namespace WebBanNuocMVC.Models.Cart
{
    public class ShoppingCart
    {
        public List<CartItem> Items { get; set; } = new List<CartItem>();

        public IDiscountStrategy DiscountStrategy { get; set; } = new NoDiscount();

        public decimal TotalAmount => Items.Sum(i => i.Subtotal);
        public decimal DiscountAmount
        {
            get
            {
                if (DiscountStrategy != null)
                {
                    _lastDiscount = DiscountStrategy.CalculateDiscount(TotalAmount);
                }
                return _lastDiscount;
            }
        }
        private decimal _lastDiscount;
        public decimal FinalAmount => TotalAmount - DiscountAmount;
        public int TotalItems => Items.Sum(i => i.Quantity);

        public void AddItem(CartItem item)
        {
            // SỬA TẠI ĐÂY: Tìm item khớp cả ID và Size
            var existingItem = Items.FirstOrDefault(i =>
                i.DrinkId == item.DrinkId && i.Size == item.Size);

            if (existingItem != null)
            {
                existingItem.Quantity += item.Quantity;
            }
            else
            {
                Items.Add(item);
            }
        }
        public void UpdateItemSize(int drinkId, string oldSize, string newSize, string newName, decimal newPrice)
        {
            var item = Items.FirstOrDefault(i => i.DrinkId == drinkId && i.Size == oldSize);
            if (item != null)
            {
                // Kiểm tra xem size mới đã tồn tại trong giỏ chưa (tránh trùng lặp)
                var existingNewSizeItem = Items.FirstOrDefault(i => i.DrinkId == drinkId && i.Size == newSize);

                if (existingNewSizeItem != null && oldSize != newSize)
                {
                    // Nếu đã có size mới, gộp số lượng vào và xóa món cũ
                    existingNewSizeItem.Quantity += item.Quantity;
                    Items.Remove(item);
                }
                else
                {
                    // Nếu chưa có, cập nhật trực tiếp thông tin từ Decorator mới truyền vào
                    item.Size = newSize;
                    item.DrinkName = newName;
                    item.Price = newPrice;
                }
            }
        }
        // SỬA TẠI ĐÂY: Thêm tham số size
        public void UpdateQuantity(int drinkId, string size, int quantity)
        {
            var item = Items.FirstOrDefault(i =>
                i.DrinkId == drinkId && i.Size == size);

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

        // SỬA TẠI ĐÂY: Thêm tham số size
        public void RemoveItem(int drinkId, string size)
        {
            Items.RemoveAll(i => i.DrinkId == drinkId && i.Size == size);
        }

        public void Clear()
        {
            Items.Clear();
        }
    }
}