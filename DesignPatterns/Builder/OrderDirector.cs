using WebBanNuocMVC.Data;
using WebBanNuocMVC.Models.Cart;

namespace WebBanNuocMVC.DesignPatterns.Builder
{
    public class OrderDirector
    {
        private readonly IOrderBuilder _orderBuilder;

        public OrderDirector(IOrderBuilder orderBuilder)
        {
            _orderBuilder = orderBuilder;
        }

        public Order BuildCheckoutOrder(
            int customerId,
            int? accountId,
            ShoppingCart cart,
            string status,
            decimal shippingFee = 20000,
            int? tableId = null)
        {
            return _orderBuilder
                .Reset()
                .WithCustomer(customerId)
                .WithAccount(accountId)
                .WithStatus(status)
                .WithOrderDate(DateTime.Now)
                .WithShippingFee(shippingFee)
                .WithTable(tableId)
                .FromCart(cart)
                .Build();
        }
    }
}