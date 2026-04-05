namespace WebBanNuocMVC.DesignPatterns.Observer
{
    public interface IOrderSubject
    {
        void Attach(IOrderObserver observer);
        void Detach(IOrderObserver observer);
        Task NotifyAsync(OrderStatusChangedEvent orderEvent);
    }
}