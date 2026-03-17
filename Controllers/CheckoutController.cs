using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanNuocMVC.Data;
using WebBanNuocMVC.Extensions;
using WebBanNuocMVC.Models.Cart;
using WebBanNuocMVC.Models.ViewModels;
using System.Security.Claims;
using WebBanNuocMVC.DesignPatterns.FactoryMethod;
using WebBanNuocMVC.DesignPatterns.FactoryMethod.ConcreteFactories;
using WebBanNuocMVC.DesignPatterns.Facade;

namespace WebBanNuocMVC.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly CoffeeShopDbContext _context;
        private readonly IPaymentFactory _paymentFactory;
        private const string CartSessionKey = "Cart";
        private const string CheckoutInfoKey = "CheckoutInfo";

        private readonly ICheckoutFacade _checkoutFacade;

       
        public CheckoutController(ICheckoutFacade checkoutFacade, CoffeeShopDbContext context)
        {
            _checkoutFacade = checkoutFacade;
            _context = context;
        }

        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>(CartSessionKey);
            if (cart == null || cart.Items.Count == 0)
            {
                TempData["Error"] = "Your cart is empty";
                return RedirectToAction("Index", "Cart");
            }

            var model = new CheckoutViewModel();
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userId, out int accountId))
                {
                    var existingOrder = _context.Orders
                        .Include(o => o.Customer)
                        .FirstOrDefault(o => o.AccountId == accountId && o.Customer != null);

                    if (existingOrder?.Customer != null)
                    {
                        model.CustomerName = existingOrder.Customer.CustomerName ?? "";
                        model.Phone = existingOrder.Customer.Phone ?? "";
                        model.Email = existingOrder.Customer.Email ?? "";
                        model.Address = existingOrder.Customer.Address ?? "";
                    }
                }
            }

            ViewBag.Cart = cart;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(CheckoutViewModel model, string? returnToAdmin = null)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>(CartSessionKey);
            if (cart == null || cart.Items.Count == 0) return RedirectToAction("Index", "Cart", new { returnToAdmin });

            if (!ModelState.IsValid)
            {
                ViewBag.Cart = cart;
                return View(model);
            }

            HttpContext.Session.SetObjectAsJson(CheckoutInfoKey, model);
            return RedirectToAction("Payment", new { returnToAdmin });
        }

        public IActionResult Payment(string? returnToAdmin = null)
        {
            var checkoutInfo = HttpContext.Session.GetObjectFromJson<CheckoutViewModel>(CheckoutInfoKey);
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>(CartSessionKey);

            if (checkoutInfo == null || cart == null)
                return RedirectToAction("Index", new { returnToAdmin });

            ViewBag.Cart = cart;
            ViewBag.CheckoutInfo = checkoutInfo;
            return View();
        }

        public async Task<IActionResult> Success(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails).ThenInclude(od => od.Drink)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null) return NotFound();
            return View(order);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessPayment(string paymentMethod, string? returnToAdmin = null)
        {
            var checkoutInfo = HttpContext.Session.GetObjectFromJson<CheckoutViewModel>("CheckoutInfo");
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");

            if (checkoutInfo == null || cart == null) return RedirectToAction("Index", "Cart", new { returnToAdmin });

            paymentMethod = "COD";

            try
            {
                var paymentUrl = await _checkoutFacade.PlaceOrderAndGetPaymentUrl(
                    checkoutInfo, cart, paymentMethod, Url, Request.Scheme);
                return Redirect(paymentUrl);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Payment", new { returnToAdmin });
            }
        }

        public async Task<IActionResult> VNPayCallback()
        {
            var queryParams = Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString());
            bool isSuccess = await _checkoutFacade.ProcessVNPayCallback(queryParams);

            if (isSuccess)
            {
                int orderId = int.Parse(queryParams["vnp_TxnRef"]);
                return RedirectToAction("Success", new { orderId });
            }

            TempData["Error"] = "Thanh toán VNPay thất bại";
            return RedirectToAction("Failed");
        }

        public async Task<IActionResult> PayPalCallback(string token, string PayerID)
        {
            int orderId = await _checkoutFacade.ProcessPayPalCallback(token, PayerID);
            if (orderId > 0) return RedirectToAction("Success", new { orderId });

            TempData["Error"] = "Thanh toán PayPal thất bại";
            return RedirectToAction("Failed");
        }

    }
}