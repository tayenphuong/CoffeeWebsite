using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanNuocMVC.Data;
using WebBanNuocMVC.Extensions;
using WebBanNuocMVC.Models.Cart;
using WebBanNuocMVC.Models.ViewModels;
using System.Security.Claims;
using WebBanNuocMVC.DesignPatterns.FactoryMethod;
using WebBanNuocMVC.DesignPatterns.FactoryMethod.ConcreteFactories;
using System.Collections.Generic;
using System.Linq;
using WebBanNuocMVC.DesignPatterns.State;
using WebBanNuocMVC.DesignPatterns.Observer;
using WebBanNuocMVC.DesignPatterns.Builder;

namespace WebBanNuocMVC.DesignPatterns.Facade
{
    public class CheckoutFacade : ICheckoutFacade
    {
        private readonly OrderDirector _orderDirector;
        private readonly IOrderSubject _orderSubject;
        private readonly CoffeeShopDbContext _context;
        private readonly IPaymentFactory _paymentFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CheckoutFacade(
         CoffeeShopDbContext context,
         IPaymentFactory paymentFactory,
         IHttpContextAccessor httpContextAccessor,
         IOrderSubject orderSubject,
         OrderDirector orderDirector)
        {
            _context = context;
            _paymentFactory = paymentFactory;
            _httpContextAccessor = httpContextAccessor;
            _orderSubject = orderSubject;
            _orderDirector = orderDirector;
        }

        public async Task<string> PlaceOrderAndGetPaymentUrl(
            CheckoutViewModel checkoutInfo,
            ShoppingCart cart,
            string paymentMethod,
            IUrlHelper urlHelper,
            string scheme)
        {
            if (!Enum.TryParse(paymentMethod, true, out PaymentMethod methodEnum))
            {
                throw new Exception("Phương thức thanh toán không hợp lệ");
            }

            // 1. Lấy hoặc Tạo khách hàng
            var customer = await GetOrCreateCustomer(checkoutInfo);

            // 2. Xác định trạng thái ban đầu
            var initialStatus = (methodEnum == PaymentMethod.COD)
     ? OrderStatusValues.Pending
     : OrderStatusValues.PendingPayment;

            // 3. Tạo Đơn hàng (Hàm này giờ đã dùng được User thông qua HttpContextAccessor)
            var order = await CreateOrder(customer, cart, initialStatus);

            // 4. Lấy Service thanh toán từ Factory
            var paymentService = _paymentFactory.GetPaymentService(methodEnum);

            // 5. Tạo các đường dẫn Callback
            var returnUrl = methodEnum switch
            {
                PaymentMethod.VnPay => urlHelper.Action("VNPayCallback", "Checkout", null, scheme),
                PaymentMethod.PayPal => urlHelper.Action("PayPalCallback", "Checkout", null, scheme),
                _ => urlHelper.Action("Success", "Checkout", new { orderId = order.OrderId }, scheme)
            };

            // 6. Tính tổng tiền (Tiền hàng sau giảm + Ship)
            var finalTotal = cart.FinalAmount + 20000;
            if (methodEnum == PaymentMethod.PayPal) finalTotal /= 25000;

            // 7. Tạo URL thanh toán cuối cùng
            var paymentUrl = paymentService.CreatePaymentUrl(
                order.OrderId,
                finalTotal,
                $"Thanh toan don hang #{order.OrderId}",
                returnUrl!
            );

            // 8. Nếu là COD, tự động dọn dẹp Session luôn
            if (methodEnum == PaymentMethod.COD)
            {
                ClearCheckoutSession();
            }

            return paymentUrl;
        }

        private async Task<Customer> GetOrCreateCustomer(CheckoutViewModel checkoutInfo)
        {
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == checkoutInfo.Email);
            if (customer == null)
            {
                customer = new Customer
                {
                    CustomerName = checkoutInfo.CustomerName,
                    Phone = checkoutInfo.Phone,
                    Email = checkoutInfo.Email,
                    Address = checkoutInfo.Address
                };
                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();
            }
            return customer;
        }

        private async Task<Order> CreateOrder(Customer customer, ShoppingCart cart, string status)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var userIdStr = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            int? accountId = null;
            if (user?.Identity?.IsAuthenticated == true && int.TryParse(userIdStr, out int uid))
            {
                accountId = uid;
            }

            var order = _orderDirector.BuildCheckoutOrder(
                customerId: customer.CustomerId,
                accountId: accountId,
                cart: cart,
                status: status,
                shippingFee: 20000
            );

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return order;
        }

        private void ClearCheckoutSession()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            session?.Remove("Cart");
            session?.Remove("CheckoutInfo");
        }

        public async Task<bool> ProcessVNPayCallback(IDictionary<string, string> queryParams)
        {
            var vnPayService = _paymentFactory.GetPaymentService(PaymentMethod.VnPay);

            // 1. Kiểm tra chữ ký
            if (!vnPayService.ValidateCallback(queryParams.ToDictionary(x => x.Key, x => x.Value)))
                return false;

            // Cách viết thủ công nhưng cực kỳ an toàn
            var responseCode = queryParams.ContainsKey("vnp_ResponseCode")
                               ? queryParams["vnp_ResponseCode"]
                               : null;

            var orderIdStr = queryParams.ContainsKey("vnp_TxnRef")
                             ? queryParams["vnp_TxnRef"]
                             : "0";
            var orderId = int.Parse(orderIdStr);

            // 2. Nếu thành công, cập nhật trạng thái
            if (responseCode == "00")
            {
                await TransitionOrderState(orderId, OrderAction.Pay); // Hàm cập nhật State đã viết ở trên
                return true;
            }

            return false;
        }

        public async Task<int> ProcessPayPalCallback(string token, string payerId)
        {
            var payPalService = _paymentFactory.GetPaymentService(PaymentMethod.PayPal);
            var queryParams = new Dictionary<string, string> { { "token", token }, { "PayerID", payerId } };

            if (!payPalService.ValidateCallback(queryParams)) return 0;

            var order = await _context.Orders
                .OrderByDescending(o => o.OrderDate)
                .FirstOrDefaultAsync(o => o.Status == OrderStatusValues.PendingPayment);

            if (order != null)
            {
                await TransitionOrderState(order.OrderId, OrderAction.Pay);
                return order.OrderId;
            }
            return 0;
        }
        // Ở đây bạn có thể tích hợp State Pattern thực thụ
        //private async Task TransitionOrderState(int orderId, string action)
        //{
        //    var order = await _context.Orders.FindAsync(orderId);
        //    if (order != null)
        //    {
        //        // Ở đây bạn có thể tích hợp State Pattern thực thụ:
        //        // var orderContext = new OrderContext(order.Status);
        //        // if (action == "Pay") orderContext.HandlePayment();
        //        // order.Status = orderContext.GetCurrentStatus();

        //        // Logic tạm thời:
        //        if (action == "Pay")
        //        {
        //            order.Status = "Paid";
        //        }

        //        await _context.SaveChangesAsync();

        //        // Sau khi thanh toán thành công, dọn dẹp session
        //        ClearCheckoutSession();
        //    }
        //}
        private async Task TransitionOrderState(int orderId, OrderAction action)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
            {
                throw new Exception("Không tìm thấy đơn hàng.");
            }

            var oldStatus = order.Status ?? string.Empty;

            var context = new OrderContext(order.Status);
            context.Handle(action);

            var newStatus = context.CurrentStatus;
            order.Status = newStatus;

            await _context.SaveChangesAsync();

            var orderEvent = new OrderStatusChangedEvent
            {
                OrderId = order.OrderId,
                OldStatus = oldStatus,
                NewStatus = newStatus,
                ChangedAt = DateTime.Now,
                CustomerEmail = order.Customer?.Email
            };

            await _orderSubject.NotifyAsync(orderEvent);

            if (action == OrderAction.Pay)
            {
                ClearCheckoutSession();
            }
        }


    }

}
