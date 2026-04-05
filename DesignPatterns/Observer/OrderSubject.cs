namespace WebBanNuocMVC.DesignPatterns.Observer
{
    public class OrderSubject : IOrderSubject
    {
        private readonly List<IOrderObserver> _observers;

        public OrderSubject(IEnumerable<IOrderObserver> observers)
        {
            _observers = observers.ToList();
        }

        public void Attach(IOrderObserver observer)
        {
            if (!_observers.Contains(observer))
            {
                _observers.Add(observer);
            }
        }

        public void Detach(IOrderObserver observer)
        {
            if (_observers.Contains(observer))
            {
                _observers.Remove(observer);
            }
        }

        public async Task NotifyAsync(OrderStatusChangedEvent orderEvent)
        {
            foreach (var observer in _observers)
            {
                await observer.UpdateAsync(orderEvent);
            }
        }
    }
}