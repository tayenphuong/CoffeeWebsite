using System.Linq;
using WebBanNuocMVC.Data;
using WebBanNuocMVC.Models.Cart;

namespace WebBanNuocMVC.DesignPatterns.Builder
{
    public class OrderBuilder : IOrderBuilder
    {
        private Order _order = null!;
        private ShoppingCart? _cart;
        private decimal _shippingFee;

        public OrderBuilder()
        {
            Reset();
        }

        public IOrderBuilder Reset()
        {
            _order = new Order
            {
                OrderDate = DateTime.Now,
                Status = "Pending"
            };

            _cart = null;
            _shippingFee = 0;
            return this;
        }

        public IOrderBuilder WithCustomer(int customerId)
        {
            _order.CustomerId = customerId;
            return this;
        }

        public IOrderBuilder WithAccount(int? accountId)
        {
            _order.AccountId = accountId;
            return this;
        }

        public IOrderBuilder WithStatus(string status)
        {
            _order.Status = status;
            return this;
        }

        public IOrderBuilder WithOrderDate(DateTime orderDate)
        {
            _order.OrderDate = orderDate;
            return this;
        }

        public IOrderBuilder WithShippingFee(decimal shippingFee)
        {
            _shippingFee = shippingFee;
            UpdateTotal();
            return this;
        }

        public IOrderBuilder WithTable(int? tableId)
        {
            _order.TableId = tableId;
            return this;
        }

        public IOrderBuilder FromCart(ShoppingCart cart)
        {
            _cart = cart;
            _order.OrderDetails.Clear();

            foreach (var item in cart.Items)
            {
                _order.OrderDetails.Add(new OrderDetail
                {
                    DrinkId = item.DrinkId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Price
                });
            }

            UpdateTotal();
            return this;
        }

        private void UpdateTotal()
        {
            var cartTotal = _cart?.FinalAmount ?? 0;
            _order.TotalAmount = cartTotal + _shippingFee;
        }

        public Order Build()
        {
            if (_order.CustomerId == null || _order.CustomerId <= 0)
            {
                throw new InvalidOperationException("Order phải có CustomerId hợp lệ.");
            }

            if (!_order.OrderDetails.Any())
            {
                throw new InvalidOperationException("Order phải có ít nhất một OrderDetail.");
            }

            var result = _order;
            Reset();
            return result;
        }
    }
}