using System.Threading.Tasks;

namespace WebBanNuocMVC.DesignPatterns.Chain
{
    public class CustomerInfoValidationHandler : CheckoutHandlerBase
    {
        public override async Task<CheckoutChainResult> HandleAsync(CheckoutProcessingContext context)
        {
            var request = context.Request;

            if (string.IsNullOrWhiteSpace(request.FullName))
            {
                return CheckoutChainResult.Fail("Vui lòng nhập họ tên người nhận.");
            }

            if (string.IsNullOrWhiteSpace(request.Phone))
            {
                return CheckoutChainResult.Fail("Vui lòng nhập số điện thoại.");
            }

            if (string.IsNullOrWhiteSpace(request.Address))
            {
                return CheckoutChainResult.Fail("Vui lòng nhập địa chỉ nhận hàng.");
            }

            if (string.IsNullOrWhiteSpace(request.PaymentMethod))
            {
                return CheckoutChainResult.Fail("Vui lòng chọn phương thức thanh toán.");
            }

            return await HandleNextAsync(context);
        }
    }
}