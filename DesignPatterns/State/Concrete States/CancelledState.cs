namespace WebBanNuocMVC.DesignPatterns.State
{
    public class CancelledState : OrderStateBase
    {
        public override string Name => OrderStatusValues.Cancelled;
    }
}