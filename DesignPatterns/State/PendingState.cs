namespace WebBanNuocMVC.DesignPatterns.State
{
    public class PendingState : OrderStateBase
    {
        public override string Name => OrderStatusValues.Pending;

        public override string StartPreparing()
            => OrderStatusValues.Preparing;

        public override string Cancel()
            => OrderStatusValues.Cancelled;
    }
}