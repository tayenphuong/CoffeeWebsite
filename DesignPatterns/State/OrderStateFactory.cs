using System;

namespace WebBanNuocMVC.DesignPatterns.State
{
    public static class OrderStateFactory
    {
        public static IOrderState Create(string? status)
        {
            return status switch
            {
                OrderStatusValues.Pending => new PendingState(),
                OrderStatusValues.PendingPayment => new PendingPaymentState(),
                OrderStatusValues.Paid => new PaidState(),
                OrderStatusValues.Preparing => new PreparingState(),
                OrderStatusValues.Completed => new CompletedState(),
                OrderStatusValues.Cancelled => new CancelledState(),
                null => new PendingState(),
                _ => throw new InvalidOperationException($"Trạng thái đơn hàng không hợp lệ: {status}")
            };
        }
    }
}