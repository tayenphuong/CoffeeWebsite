using WebBanNuocMVC.DesignPatterns.Singleton;

namespace WebBanNuocMVC.DesignPatterns.Observer
{
    public class OrderLoggerObserver : IOrderObserver
    {
        private readonly ILoggerService _logger;

        public OrderLoggerObserver(ILoggerService logger)
        {
            _logger = logger;
        }

        public Task UpdateAsync(OrderStatusChangedEvent orderEvent)
        {
            _logger.LogInfo(
                $"Order #{orderEvent.OrderId} changed status from '{orderEvent.OldStatus}' to '{orderEvent.NewStatus}' at {orderEvent.ChangedAt:dd/MM/yyyy HH:mm:ss}."
            );

            return Task.CompletedTask;
        }
    }
}