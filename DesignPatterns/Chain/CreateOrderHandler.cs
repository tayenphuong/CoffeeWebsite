using System.Threading.Tasks;

namespace WebBanNuocMVC.DesignPatterns.Chain
{
    public class CreateOrderHandler : CheckoutHandlerBase
    {
        public override async Task<CheckoutChainResult> HandleAsync(CheckoutProcessingContext context)
        {
            return await HandleNextAsync(context);
        }
    }
}