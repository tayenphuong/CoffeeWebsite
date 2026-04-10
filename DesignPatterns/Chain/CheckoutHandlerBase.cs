using System.Threading.Tasks;

namespace WebBanNuocMVC.DesignPatterns.Chain
{
    public abstract class CheckoutHandlerBase : ICheckoutHandler
    {
        private ICheckoutHandler? _next;

        public ICheckoutHandler SetNext(ICheckoutHandler next)
        {
            _next = next;
            return next;
        }

        public abstract Task<CheckoutChainResult> HandleAsync(CheckoutProcessingContext context);

        protected async Task<CheckoutChainResult> HandleNextAsync(CheckoutProcessingContext context)
        {
            if (_next == null)
            {
                return CheckoutChainResult.Success("Chuỗi xử lý hoàn tất.");
            }

            return await _next.HandleAsync(context);
        }
    }
}