using System;

namespace WebBanNuocMVC.DesignPatterns.State
{
    public abstract class OrderStateBase : IOrderState
    {
        public abstract string Name { get; }

        public virtual string Pay()
            => throw new InvalidOperationException($"Không thể thanh toán khi đơn hàng đang ở trạng thái '{Name}'.");

        public virtual string StartPreparing()
            => throw new InvalidOperationException($"Không thể chuyển sang Preparing khi đơn hàng đang ở trạng thái '{Name}'.");

        public virtual string Complete()
            => throw new InvalidOperationException($"Không thể hoàn tất đơn hàng khi đang ở trạng thái '{Name}'.");

        public virtual string Cancel()
            => throw new InvalidOperationException($"Không thể hủy đơn hàng khi đang ở trạng thái '{Name}'.");
    }
}