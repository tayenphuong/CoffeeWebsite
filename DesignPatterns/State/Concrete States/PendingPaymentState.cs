namespace WebBanNuocMVC.DesignPatterns.State
{
    public class PendingPaymentState : OrderStateBase
    {
        public override string Name => OrderStatusValues.PendingPayment;

        public override string Pay()
            => OrderStatusValues.Paid;

        public override string Cancel()
            => OrderStatusValues.Cancelled;
    }
}