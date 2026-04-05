using WebBanNuocMVC.Data;
using WebBanNuocMVC.Models.Cart;

namespace WebBanNuocMVC.DesignPatterns.Builder
{
    public interface IOrderBuilder
    {
        IOrderBuilder Reset();
        IOrderBuilder WithCustomer(int customerId);
        IOrderBuilder WithAccount(int? accountId);
        IOrderBuilder WithStatus(string status);
        IOrderBuilder WithOrderDate(DateTime orderDate);
        IOrderBuilder WithShippingFee(decimal shippingFee);
        IOrderBuilder WithTable(int? tableId);
        IOrderBuilder FromCart(ShoppingCart cart);
        Order Build();
    }
}