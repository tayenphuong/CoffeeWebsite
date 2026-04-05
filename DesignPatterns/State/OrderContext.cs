namespace WebBanNuocMVC.DesignPatterns.State
{
    public class OrderContext
    {
        private IOrderState _currentState;

        public OrderContext(string? currentStatus)
        {
            _currentState = OrderStateFactory.Create(currentStatus);
        }

        public string CurrentStatus => _currentState.Name;

        public void Handle(OrderAction action)
        {
            var nextStatus = action switch
            {
                OrderAction.Pay => _currentState.Pay(),
                OrderAction.StartPreparing => _currentState.StartPreparing(),
                OrderAction.Complete => _currentState.Complete(),
                OrderAction.Cancel => _currentState.Cancel(),
                _ => _currentState.Name
            };

            _currentState = OrderStateFactory.Create(nextStatus);
        }
    }
}