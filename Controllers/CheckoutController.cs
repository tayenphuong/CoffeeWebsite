using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebBanNuocMVC.Data;
using WebBanNuocMVC.DesignPatterns.Chain;
using WebBanNuocMVC.DesignPatterns.Facade;
using WebBanNuocMVC.Extensions;
using WebBanNuocMVC.Models.Cart;
using WebBanNuocMVC.Models.ViewModels;

namespace WebBanNuocMVC.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly CoffeeShopDbContext _context;
        private readonly ICheckoutFacade _checkoutFacade;
        private readonly CheckoutChainService _checkoutChainService; // Design Pattern: Chain of Responsibility

        private const string CartSessionKey = "Cart";
        private const string CheckoutInfoKey = "CheckoutInfo";

        public CheckoutController(
            ICheckoutFacade checkoutFacade,
            CheckoutChainService checkoutChainService,
            CoffeeShopDbContext context)
        {
            _checkoutFacade = checkoutFacade;
            _checkoutChainService = checkoutChainService;
            _context = context;
        }

        [HttpGet]
        public IActionResult Index(string? returnToAdmin = null)
        {
            var cart = GetCurrentCart();
            if (cart == null || cart.Items.Count == 0)
            {
                TempData["Error"] = "Your cart is empty";
                return RedirectToAction("Index", "Cart", new { returnToAdmin });
            }

            // Logic tự động điền thông tin khách hàng cũ
            var model = BuildPrefilledCheckoutModel();

            ViewBag.Cart = cart;
            ViewBag.ReturnToAdmin = returnToAdmin;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(CheckoutViewModel model, string? returnToAdmin = null)
        {
            var cart = GetCurrentCart();
            if (cart == null || cart.Items.Count == 0)
                return RedirectToAction("Index", "Cart", new { returnToAdmin });

            if (!ModelState.IsValid)
            {
                ViewBag.Cart = cart;
                ViewBag.ReturnToAdmin = returnToAdmin;
                return View(model);
            }

            HttpContext.Session.SetObjectAsJson(CheckoutInfoKey, model);
            return RedirectToAction(nameof(Payment), new { returnToAdmin });
        }

        [HttpGet]
        public IActionResult Payment(string? returnToAdmin = null)
        {
            var checkoutInfo = GetCurrentCheckoutInfo();
            var cart = GetCurrentCart();

            if (checkoutInfo == null || cart == null)
            {
                TempData["Error"] = "Checkout information not found";
                return RedirectToAction(nameof(Index), new { returnToAdmin });
            }

            ViewBag.Cart = cart;
            ViewBag.CheckoutInfo = checkoutInfo;
            ViewBag.ReturnToAdmin = returnToAdmin;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessPayment(string paymentMethod, string? returnToAdmin = null)
        {
            var checkoutInfo = GetCurrentCheckoutInfo();
            var cart = GetCurrentCart();

            if (checkoutInfo == null || cart == null)
                return RedirectToAction("Index", "Cart", new { returnToAdmin });

            if (string.IsNullOrWhiteSpace(paymentMethod))
            {
                TempData["Error"] = "Please select a payment method";
                return RedirectToAction(nameof(Payment), new { returnToAdmin });
            }

            try
            {
                // --- BƯỚC 1: CHẠY CHAIN OF RESPONSIBILITY ---
                // Dùng để Validate số điện thoại, địa chỉ, kiểm tra tồn kho trước khi tạo Order
                var chainRequest = BuildChainRequest(checkoutInfo, cart, paymentMethod);
                var chainResult = await _checkoutChainService.ExecuteAsync(chainRequest);

                if (!chainResult.Succeeded)
                {
                    TempData["Error"] = chainResult.Message;
                    return RedirectToAction(nameof(Payment), new { returnToAdmin });
                }

                // --- BƯỚC 2: GỌI FACADE ĐỂ XỬ LÝ THANH TOÁN ---
                var paymentUrl = await _checkoutFacade.PlaceOrderAndGetPaymentUrl(
                    checkoutInfo, cart, paymentMethod, Url, Request.Scheme);

                if (string.IsNullOrWhiteSpace(paymentUrl))
                {
                    throw new Exception("Cannot initialize payment URL.");
                }

                return Redirect(paymentUrl);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Payment), new { returnToAdmin });
            }
        }

        [HttpGet]
        public async Task<IActionResult> VNPayCallback()
        {
            var queryParams = Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString());
            bool isSuccess = await _checkoutFacade.ProcessVNPayCallback(queryParams);

            if (isSuccess && queryParams.TryGetValue("vnp_TxnRef", out var orderIdStr) && int.TryParse(orderIdStr, out int orderId))
            {
                ClearCheckoutSession(); // Dọn dẹp session sau khi thành công
                return RedirectToAction(nameof(Success), new { orderId });
            }

            TempData["Error"] = "Thanh toán VNPay thất bại hoặc chữ ký không hợp lệ.";
            return RedirectToAction(nameof(Failed));
        }

        [HttpGet]
        public async Task<IActionResult> PayPalCallback(string token, string PayerID)
        {
            int orderId = await _checkoutFacade.ProcessPayPalCallback(token, PayerID);

            if (orderId > 0)
            {
                ClearCheckoutSession();
                return RedirectToAction(nameof(Success), new { orderId });
            }

            TempData["Error"] = "Thanh toán PayPal thất bại";
            return RedirectToAction(nameof(Failed));
        }

        [HttpGet]
        public async Task<IActionResult> Success(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails).ThenInclude(od => od.Drink)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null) return NotFound();
            return View(order);
        }

        [HttpGet]
        public IActionResult Failed() => View();

        #region Helpers
        private ShoppingCart? GetCurrentCart() => HttpContext.Session.GetObjectFromJson<ShoppingCart>(CartSessionKey);
        private CheckoutViewModel? GetCurrentCheckoutInfo() => HttpContext.Session.GetObjectFromJson<CheckoutViewModel>(CheckoutInfoKey);

        private void ClearCheckoutSession()
        {
            HttpContext.Session.Remove(CartSessionKey);
            HttpContext.Session.Remove(CheckoutInfoKey);
        }

        private CheckoutViewModel BuildPrefilledCheckoutModel()
        {
            var model = new CheckoutViewModel();
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userId, out int accountId))
                {
                    var customer = _context.Customers.FirstOrDefault(c => _context.Orders.Any(o => o.AccountId == accountId && o.CustomerId == c.CustomerId));
                    if (customer != null)
                    {
                        model.CustomerName = customer.CustomerName ?? "";
                        model.Phone = customer.Phone ?? "";
                        model.Email = customer.Email ?? "";
                        model.Address = customer.Address ?? "";
                    }
                }
            }
            return model;
        }

        private CheckoutChainRequest BuildChainRequest(CheckoutViewModel info, ShoppingCart cart, string method)
        {
            return new CheckoutChainRequest
            {
                FullName = info.CustomerName ?? "",
                Phone = info.Phone ?? "",
                Address = info.Address ?? "",
                PaymentMethod = method,
                CartItems = cart.Items.Select(i => new CheckoutCartItem { DrinkId = i.DrinkId, Quantity = i.Quantity }).ToList()
            };
        }
        #endregion
    }
}