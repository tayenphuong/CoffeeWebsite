using WebBanNuocMVC.Data;
using WebBanNuocMVC.DesignPatterns.State;
using Microsoft.EntityFrameworkCore; // Để sử dụng AsNoTracking() trong ExecuteAsync() để lấy trạng thái cũ mà không theo dõi thay đổi.
// OrderStatusCommand.cs (Lớp dùng chung cho các lệnh đổi trạng thái)
namespace WebBanNuocMVC.DesignPatterns.Command
{
    public class OrderStatusCommand : IOrderCommand
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly Func<int, OrderAction, Task<bool>> _changeStateFunc;
        private readonly int _orderId;
        private readonly OrderAction _action;
        private string _oldStatus;

        public string Description => $"Thay đổi đơn hàng #{_orderId} sang {_action}";

        public OrderStatusCommand(IServiceScopeFactory scopeFactory, Func<int, OrderAction, Task<bool>> changeStateFunc, int orderId, OrderAction action)
        {
            _scopeFactory = scopeFactory;
            _changeStateFunc = changeStateFunc;
            _orderId = orderId;
            _action = action;
        }

        public async Task ExecuteAsync()
        {
            // Tạo một scope tạm thời để lấy trạng thái cũ
            using (var scope = _scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<CoffeeShopDbContext>();
                var order = await db.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.OrderId == _orderId);
                _oldStatus = order?.Status;
            }

            await _changeStateFunc(_orderId, _action);
        }

        public async Task UndoAsync()
        {
            // Tạo một scope tạm thời để quay lại trạng thái cũ
            using (var scope = _scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<CoffeeShopDbContext>();
                var order = await db.Orders.FindAsync(_orderId);
                if (order != null)
                {
                    order.Status = _oldStatus;
                    await db.SaveChangesAsync();
                }
            }
        }
    }
}

