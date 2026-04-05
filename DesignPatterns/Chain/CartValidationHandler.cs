using System.Linq;
using System.Threading.Tasks;

namespace WebBanNuocMVC.DesignPatterns.Chain
{
    public class CartValidationHandler : CheckoutHandlerBase
    {
        public override async Task<CheckoutChainResult> HandleAsync(CheckoutProcessingContext context)
        {
            var cartItems = context.Request.CartItems;

            if (cartItems == null || !cartItems.Any())
            {
                return CheckoutChainResult.Fail("Giỏ hàng đang trống.");
            }

            if (cartItems.Any(x => x.Quantity <= 0))
            {
                return CheckoutChainResult.Fail("Số lượng sản phẩm trong giỏ hàng không hợp lệ.");
            }

            if (cartItems.Any(x => x.DrinkId <= 0))
            {
                return CheckoutChainResult.Fail("Có sản phẩm không hợp lệ trong giỏ hàng.");
            }

            return await HandleNextAsync(context);
        }
    }
}