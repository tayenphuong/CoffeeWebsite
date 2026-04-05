namespace WebBanNuocMVC.DesignPatterns.State
{
    public class CompletedState : OrderStateBase
    {
        public override string Name => OrderStatusValues.Completed;
    }
}