using Microsoft.AspNetCore.Cors.Infrastructure;
using WebBanNuocMVC.DesignPatterns.FactoryMethod;

namespace WebBanNuocMVC.DesignPatterns.FactoryMethod.ConcreteFactories
{
    public class PaymentFactory : IPaymentFactory
        {
            private readonly IServiceProvider _serviceProvider;

            public PaymentFactory(IServiceProvider serviceProvider)
            {
                _serviceProvider = serviceProvider;
            }

            public IPaymentService GetPaymentService(PaymentMethod method)
            {
                return method switch
                {
                    PaymentMethod.VnPay => _serviceProvider.GetRequiredService<VNPayService>(),
                    PaymentMethod.PayPal => _serviceProvider.GetRequiredService<PayPalService>(),
                    PaymentMethod.COD => _serviceProvider.GetRequiredService<CODService>(),
                    _ => throw new NotSupportedException("Phương thức thanh toán không hỗ trợ")
                };
            }
        }
    
}