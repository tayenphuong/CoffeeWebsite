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
        private readonly CheckoutChainService _checkoutChainService;

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

            if (cart == null || cart.Items == null || cart.Items.Count == 0)
            {
                TempData["Error"] = "Your cart is empty";
                return RedirectToAction("Index", "Cart", new { returnToAdmin });
            }

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

            if (cart == null || cart.Items == null || cart.Items.Count == 0)
            {
                TempData["Error"] = "Your cart is empty";
                return RedirectToAction("Index", "Cart", new { returnToAdmin });
            }

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

            if (checkoutInfo == null || cart == null || cart.Items == null || cart.Items.Count == 0)
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

            if (checkoutInfo == null || cart == null || cart.Items == null || cart.Items.Count == 0)
            {
                TempData["Error"] = "Checkout data not found";
                return RedirectToAction("Index", "Cart", new { returnToAdmin });
            }

            if (string.IsNullOrWhiteSpace(paymentMethod))
            {
                TempData["Error"] = "Please select a payment method";
                return RedirectToAction(nameof(Payment), new { returnToAdmin });
            }

            try
            {
                var chainRequest = BuildChainRequest(checkoutInfo, cart, paymentMethod);
                var chainResult = await _checkoutChainService.ExecuteAsync(chainRequest);

                if (!chainResult.Succeeded)
                {
                    TempData["Error"] = chainResult.Message;
                    return RedirectToAction(nameof(Payment), new { returnToAdmin });
                }

                var paymentUrl = await _checkoutFacade.PlaceOrderAndGetPaymentUrl(
                    checkoutInfo,
                    cart,
                    paymentMethod,
                    Url,
                    Request.Scheme);

                if (string.IsNullOrWhiteSpace(paymentUrl))
                {
                    TempData["Error"] = "Cannot initialize payment";
                    return RedirectToAction(nameof(Payment), new { returnToAdmin });
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
        public async Task<IActionResult> Success(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Drink)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
            {
                return NotFound();
            }

            ClearCheckoutSession();
            return View(order);
        }

        [HttpGet]
        public IActionResult Failed()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> VNPayCallback()
        {
            var queryParams = Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString());
            bool isSuccess = await _checkoutFacade.ProcessVNPayCallback(queryParams);

            if (isSuccess)
            {
                if (queryParams.TryGetValue("vnp_TxnRef", out var txnRef) && int.TryParse(txnRef, out int orderId))
                {
                    return RedirectToAction(nameof(Success), new { orderId });
                }

                TempData["Error"] = "Cannot parse order id from VNPay";
                return RedirectToAction(nameof(Failed));
            }

            TempData["Error"] = "Thanh toán VNPay thất bại";
            return RedirectToAction(nameof(Failed));
        }

        [HttpGet]
        public async Task<IActionResult> PayPalCallback(string token, string PayerID)
        {
            int orderId = await _checkoutFacade.ProcessPayPalCallback(token, PayerID);

            if (orderId > 0)
            {
                return RedirectToAction(nameof(Success), new { orderId });
            }

            TempData["Error"] = "Thanh toán PayPal thất bại";
            return RedirectToAction(nameof(Failed));
        }

        private ShoppingCart? GetCurrentCart()
        {
            return HttpContext.Session.GetObjectFromJson<ShoppingCart>(CartSessionKey);
        }

        private CheckoutViewModel? GetCurrentCheckoutInfo()
        {
            return HttpContext.Session.GetObjectFromJson<CheckoutViewModel>(CheckoutInfoKey);
        }

        private void ClearCheckoutSession()
        {
            HttpContext.Session.Remove(CartSessionKey);
            HttpContext.Session.Remove(CheckoutInfoKey);
        }

        private CheckoutViewModel BuildPrefilledCheckoutModel()
        {
            var model = new CheckoutViewModel();

            if (User.Identity?.IsAuthenticated != true)
            {
                return model;
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userId, out int accountId))
            {
                return model;
            }

            var existingOrder = _context.Orders
                .Include(o => o.Customer)
                .FirstOrDefault(o => o.AccountId == accountId && o.Customer != null);

            if (existingOrder?.Customer == null)
            {
                return model;
            }

            model.CustomerName = existingOrder.Customer.CustomerName ?? string.Empty;
            model.Phone = existingOrder.Customer.Phone ?? string.Empty;
            model.Email = existingOrder.Customer.Email ?? string.Empty;
            model.Address = existingOrder.Customer.Address ?? string.Empty;

            return model;
        }

        private CheckoutChainRequest BuildChainRequest(
            CheckoutViewModel checkoutInfo,
            ShoppingCart cart,
            string paymentMethod)
        {
            var request = new CheckoutChainRequest
            {
                FullName = checkoutInfo.CustomerName ?? string.Empty,
                Phone = checkoutInfo.Phone ?? string.Empty,
                Address = checkoutInfo.Address ?? string.Empty,
                PaymentMethod = paymentMethod,
                CartItems = new List<CheckoutCartItem>()
            };

            foreach (var item in cart.Items)
            {
                request.CartItems.Add(new CheckoutCartItem
                {
                    DrinkId = item.DrinkId,
                    Quantity = item.Quantity
                });
            }

            return request;
        }
    }
}