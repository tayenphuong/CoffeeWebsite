using System.Linq;
using System.Threading.Tasks;

namespace WebBanNuocMVC.DesignPatterns.Chain
{
    public class OrderPricingHandler : CheckoutHandlerBase
    {
        public override async Task<CheckoutChainResult> HandleAsync(CheckoutProcessingContext context)
        {
            decimal total = 0;

            foreach (var item in context.Request.CartItems)
            {
                if (!context.CurrentPrices.ContainsKey(item.DrinkId))
                {
                    return CheckoutChainResult.Fail("Không thể tính tổng tiền đơn hàng.");
                }

                total += context.CurrentPrices[item.DrinkId] * item.Quantity;
            }

            if (total <= 0)
            {
                return CheckoutChainResult.Fail("Tổng tiền đơn hàng không hợp lệ.");
            }

            context.CalculatedTotal = total;

            return await HandleNextAsync(context);
        }
    }
}