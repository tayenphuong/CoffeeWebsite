using System.Threading.Tasks;
using WebBanNuocMVC.Data;

namespace WebBanNuocMVC.DesignPatterns.Chain
{
    public class CheckoutChainService
    {
        private readonly CoffeeShopDbContext _context;

        public CheckoutChainService(CoffeeShopDbContext context)
        {
            _context = context;
        }

        public async Task<CheckoutChainResult> ExecuteAsync(CheckoutChainRequest request)
        {
            var processingContext = new CheckoutProcessingContext
            {
                Request = request
            };

            var customerInfoHandler = new CustomerInfoValidationHandler();
            var cartValidationHandler = new CartValidationHandler();
            var drinkAvailabilityHandler = new DrinkAvailabilityHandler(_context);
            var orderPricingHandler = new OrderPricingHandler();

            customerInfoHandler.SetNext(cartValidationHandler);
            cartValidationHandler.SetNext(drinkAvailabilityHandler);
            drinkAvailabilityHandler.SetNext(orderPricingHandler);

            return await customerInfoHandler.HandleAsync(processingContext);
        }
    }
}