namespace WebBanNuocMVC.DesignPatterns.FactoryMethod
{
    public interface IPaymentService
    {
        string CreatePaymentUrl(int orderId, decimal amount, string orderInfo, string returnUrl);
        bool ValidateCallback(Dictionary<string, string> queryParams);
    }
}
