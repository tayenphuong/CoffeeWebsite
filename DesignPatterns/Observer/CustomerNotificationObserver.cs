using WebBanNuocMVC.DesignPatterns.Observer;
using WebBanNuocMVC.DesignPatterns.Adapter;

public class CustomerNotificationObserver : IOrderObserver
{
    private readonly IEnumerable<INotificationAdapter> _adapters;

    // DI sẽ tự động bơm TẤT CẢ các Adapter đã đăng ký vào List này
    public CustomerNotificationObserver(IEnumerable<INotificationAdapter> adapters)
    {
        _adapters = adapters;
    }

    public async Task UpdateAsync(OrderStatusChangedEvent orderEvent)
    {
        if (orderEvent.NewStatus == "Completed")
        {
            string subject = "Coffee Shop - Đơn hàng hoàn tất";
            string message = $"Đơn hàng #{orderEvent.OrderId} của bạn đã sẵn sàng!";

            foreach (var adapter in _adapters)
            {
                try
                {
                    // Tùy biến recipient cho từng loại Adapter
                    string recipient = adapter switch
                    {
                        EmailNotificationAdapter => !string.IsNullOrEmpty(orderEvent.CustomerEmail)
                             ? orderEvent.CustomerEmail
                             : "yenphuongtyp24@gmail.com",
                        TelegramNotificationAdapter => "7631953076",
                        _ => ""
                    };

                    if (!string.IsNullOrEmpty(recipient))
                    {
                        await adapter.SendAsync(recipient, subject, message);
                    }
                }
                catch (Exception ex)
                {
                    // Tránh việc 1 Adapter lỗi (như sai email) làm chết cả luồng gửi
                    Console.WriteLine($"Lỗi khi gửi qua {adapter.GetType().Name}: {ex.Message}");
                }

            }
        }
    }
}