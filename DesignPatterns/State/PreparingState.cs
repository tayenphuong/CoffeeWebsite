namespace WebBanNuocMVC.DesignPatterns.State
{
    public class PreparingState : OrderStateBase
    {
        public override string Name => OrderStatusValues.Preparing;

        public override string Complete()
            => OrderStatusValues.Completed;
    }
}