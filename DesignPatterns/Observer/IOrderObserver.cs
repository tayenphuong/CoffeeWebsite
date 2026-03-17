namespace WebBanNuocMVC.DesignPatterns.Observer
{
    public interface IOrderObserver
    {
        Task UpdateAsync(OrderStatusChangedEvent orderEvent);
    }
}