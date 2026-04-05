namespace WebBanNuocMVC.DesignPatterns.State
{
    public class PaidState : OrderStateBase
    {
        public override string Name => OrderStatusValues.Paid;

        public override string StartPreparing()
            => OrderStatusValues.Preparing;

        public override string Cancel()
            => OrderStatusValues.Cancelled;
    }
}