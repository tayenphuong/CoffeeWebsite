using System.Threading.Tasks;

namespace WebBanNuocMVC.DesignPatterns.Chain
{
    public interface ICheckoutHandler
    {
        ICheckoutHandler SetNext(ICheckoutHandler next);
        Task<CheckoutChainResult> HandleAsync(CheckoutProcessingContext context);
    }
}