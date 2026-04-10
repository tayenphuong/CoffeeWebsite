using System.Collections.Generic;

namespace WebBanNuocMVC.DesignPatterns.Chain
{
    public class CheckoutChainRequest
    {
        public int? CustomerId { get; set; }

        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;

        public List<CheckoutCartItem> CartItems { get; set; } = new();
    }

    public class CheckoutCartItem
    {
        public int DrinkId { get; set; }
        public string DrinkName { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }
}