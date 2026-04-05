using Microsoft.AspNetCore.Mvc;
using WebBanNuocMVC.Models.Cart;
using WebBanNuocMVC.Models.ViewModels;
namespace WebBanNuocMVC.DesignPatterns.Facade
{

    public interface ICheckoutFacade
    {
        Task<string> PlaceOrderAndGetPaymentUrl(
            CheckoutViewModel checkoutInfo,
            ShoppingCart cart,
            string paymentMethod,
            IUrlHelper urlHelper,
            string scheme);

        // Hàm mới cho Callback
        Task<bool> ProcessVNPayCallback(IDictionary<string, string> queryParams);
        Task<int> ProcessPayPalCallback(string token, string payerId);
    }

   
}
