using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanNuocMVC.Data;
using WebBanNuocMVC.DesignPatterns.Decorator;
using WebBanNuocMVC.DesignPatterns.Strategy;
using WebBanNuocMVC.Extensions;
using WebBanNuocMVC.Models.Cart;

namespace WebBanNuocMVC.Controllers
{
    public class CartController : Controller
    {
        private readonly CoffeeShopDbContext _context;
        private const string CartSessionKey = "Cart";

        public CartController(CoffeeShopDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>(CartSessionKey) 
                       ?? new ShoppingCart();
            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int drinkId, int quantity = 1, string size = "S")
        {
            var drinkData = await _context.Drinks.FindAsync(drinkId);
            if (drinkData == null)
            {
                return Json(new { success = false, message = "Product not found" });
            }

            // --- ÁP DỤNG DECORATOR TẠI ĐÂY ---
            // 1. Khởi tạo đối tượng gốc (Size S mặc định)
            IDrink decoratedDrink = new BaseDrink(drinkData);

            // 2. "Gói" thêm lớp trang trí tùy theo size khách chọn
            if (size == "M")
            {
                decoratedDrink = new SizeMDecorator(decoratedDrink);
            }
            else if (size == "L")
            {
                decoratedDrink = new SizeLDecorator(decoratedDrink);
            }

            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>(CartSessionKey)
                       ?? new ShoppingCart();

            // 3. Sử dụng thông tin đã qua Decorator để thêm vào giỏ hàng
            cart.AddItem(new CartItem
            {
                // Lưu ý: DrinkId của CartItem nên là một chuỗi hoặc kết hợp ID+Size 
                // để phân biệt "Cafe (S)" và "Cafe (L)" là 2 dòng khác nhau trong giỏ hàng
                DrinkId = drinkData.DrinkId,
                Size = size, // Thêm thuộc tính Size vào CartItem nếu cần
                DrinkName = decoratedDrink.GetName(), // Vd: "Cà phê đen (Size L)"
                Price = decoratedDrink.GetPrice(),    // Vd: 30000
                Quantity = quantity,
                Image = drinkData.Image
            });

            HttpContext.Session.SetObjectAsJson(CartSessionKey, cart);

            return Json(new
            {
                success = true,
                cartCount = cart.TotalItems,
                message = $"Đã thêm {decoratedDrink.GetName()} vào giỏ hàng"
            });
        }
        [HttpPost]
        public async Task<IActionResult> UpdateSize(int drinkId, string oldSize, string newSize)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>(CartSessionKey);
            if (cart == null) return Json(new { success = false });

            // 1. Lấy dữ liệu gốc từ Database
            var drinkData = await _context.Drinks.FindAsync(drinkId);
            if (drinkData == null) return Json(new { success = false });

            // 2. ÁP DỤNG DECORATOR để lấy thông tin mới
            IDrink decoratedDrink = new BaseDrink(drinkData);
            if (newSize == "M") decoratedDrink = new SizeMDecorator(decoratedDrink);
            else if (newSize == "L") decoratedDrink = new SizeLDecorator(decoratedDrink);

            // 3. Cập nhật vào giỏ hàng
            cart.UpdateItemSize(drinkId, oldSize, newSize, decoratedDrink.GetName(), decoratedDrink.GetPrice());

            // 4. Lưu lại Session
            HttpContext.Session.SetObjectAsJson(CartSessionKey, cart);

            return Json(new
            {
                success = true,
                message = "Đã đổi sang " + decoratedDrink.GetName(),
                finalTotal = cart.FinalAmount // Trả về tổng tiền mới để cập nhật UI
            });
        }
        [HttpPost]
        public IActionResult UpdateCart(int drinkId, string size, int quantity)
        {
            // 1. Lấy giỏ hàng từ Session
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>(CartSessionKey);
            if (cart == null) return Json(new { success = false, message = "Giỏ hàng trống" });

            // 2. Gọi hàm update trong Model (Hàm này bạn đã sửa có tham số size rồi)
            cart.UpdateQuantity(drinkId, size, quantity);

            // 3. QUAN TRỌNG: Lưu lại vào Session ngay lập tức để đồng bộ dữ liệu
            HttpContext.Session.SetObjectAsJson(CartSessionKey, cart);

            // 4. Tìm lại item vừa update để lấy Subtotal mới trả về cho UI
            var updatedItem = cart.Items.FirstOrDefault(i => i.DrinkId == drinkId && i.Size == size);

            return Json(new
            {
                success = true,
                newTotal = cart.TotalAmount,
                discountAmount = cart.DiscountAmount,
                finalTotal = cart.FinalAmount + 20000, // Tổng cuối cùng bao gồm ship
                cartCount = cart.TotalItems,
                itemSubtotal = updatedItem?.Subtotal ?? 0
            });
        }

        [HttpPost]
        public IActionResult RemoveFromCart(int drinkId, string size) // Thêm tham số size
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>(CartSessionKey);
            if (cart != null)
            {
                cart.RemoveItem(drinkId, size); // Gọi hàm RemoveItem đã sửa
                HttpContext.Session.SetObjectAsJson(CartSessionKey, cart);
            }

            return RedirectToAction("Index");
        }

        public IActionResult GetCartCount()
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>(CartSessionKey);
            return Json(new { count = cart?.TotalItems ?? 0 });
        }

        [HttpPost]
        public IActionResult ApplyDiscount(string promoCode)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>(CartSessionKey);
            if (cart == null) return Json(new { success = false, message = "Cart empty" });

            // logic Strategy: Chọn thuật toán dựa trên mã nhập vào
            IDiscountStrategy strategy = promoCode?.ToUpper() switch
            {
                "MEM10" => new PercentageDiscount(10),       // Giảm 10% cho Member
                "HELLO" => new FixedAmountDiscount(20000),   // Giảm 20k cho người mới
                _ => new NoDiscount()
            };

            // Gán Strategy vào context (Giỏ hàng)
            cart.DiscountStrategy = strategy;

            // Lưu lại vào Session
            HttpContext.Session.SetObjectAsJson(CartSessionKey, cart);

            return Json(new
            {
                success = true,
                discountName = strategy.GetDescription(),
                discountAmount = cart.DiscountAmount,
                finalTotal = cart.FinalAmount,
                message = "Áp dụng mã thành công!"
            });
        }
    }
}
