using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanNuocMVC.Data;
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
        public async Task<IActionResult> AddToCart(int drinkId, int quantity = 1)
        {
            var drink = await _context.Drinks.FindAsync(drinkId);
            if (drink == null)
            {
                return Json(new { success = false, message = "Product not found" });
            }

            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>(CartSessionKey) 
                       ?? new ShoppingCart();

            cart.AddItem(new CartItem
            {
                DrinkId = drink.DrinkId,
                DrinkName = drink.DrinkName ?? "",
                Price = drink.Price ?? 0,
                Quantity = quantity,
                Image = drink.Image
            });

            HttpContext.Session.SetObjectAsJson(CartSessionKey, cart);

            return Json(new
            {
                success = true,
                cartCount = cart.TotalItems,
                message = "Added to cart successfully"
            });
        }

        [HttpPost]
        public IActionResult UpdateCart(int drinkId, int quantity)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>(CartSessionKey);
            if (cart != null)
            {
                cart.UpdateQuantity(drinkId, quantity);

                // Strategy Pattern sẽ tự động tính toán lại khi thuộc tính DiscountAmount được gọi
                HttpContext.Session.SetObjectAsJson(CartSessionKey, cart);

                return Json(new
                {
                    success = true,
                    newTotal = cart.TotalAmount,     // Tổng chưa giảm
                    discountAmount = cart.DiscountAmount, // Tiền giảm mới
                    finalTotal = cart.FinalAmount,   // Tổng đã giảm
                    cartCount = cart.TotalItems,
                    itemSubtotal = cart.Items.FirstOrDefault(i => i.DrinkId == drinkId)?.Subtotal ?? 0
                });
            }
            return Json(new { success = false, message = "Cart not found" });
        }

        [HttpPost]
        public IActionResult RemoveFromCart(int drinkId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>(CartSessionKey);
            if (cart != null)
            {
                cart.RemoveItem(drinkId);
                HttpContext.Session.SetObjectAsJson(CartSessionKey, cart);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult ClearCart()
        {
            HttpContext.Session.Remove(CartSessionKey);
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
