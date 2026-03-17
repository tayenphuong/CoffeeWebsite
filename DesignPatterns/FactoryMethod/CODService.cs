namespace WebBanNuocMVC.DesignPatterns.FactoryMethod
{
    public class CODService : IPaymentService
    {
        public string CreatePaymentUrl(int orderId, decimal amount, string orderInfo, string returnUrl)
            => returnUrl; // COD trả về trực tiếp link thành công
        public bool ValidateCallback(Dictionary<string, string> queryParams) => true;
    }
}
