using WebBanNuocMVC.DesignPatterns.Singleton;

namespace WebBanNuocMVC.DesignPatterns.Observer
{
    public class OrderAdminNotificationObserver : IOrderObserver
    {
        private readonly ILoggerService _logger;

        public OrderAdminNotificationObserver(ILoggerService logger)
        {
            _logger = logger;
        }

        public Task UpdateAsync(OrderStatusChangedEvent orderEvent)
        {
            var message =
                $"[ADMIN NOTIFY] Order #{orderEvent.OrderId} vừa chuyển từ '{orderEvent.OldStatus}' sang '{orderEvent.NewStatus}'.";

            _logger.LogInfo(message);

            return Task.CompletedTask;
        }
    }
}