namespace WebBanNuocMVC.DesignPatterns.FactoryMethod.ConcreteFactories
{
    public interface IPaymentFactory
    {
        IPaymentService GetPaymentService(PaymentMethod method);
    }
}
