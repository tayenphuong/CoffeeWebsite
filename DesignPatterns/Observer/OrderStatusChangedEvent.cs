namespace WebBanNuocMVC.DesignPatterns.Observer
{
    public class OrderStatusChangedEvent
    {
        public int OrderId { get; set; }
        public string OldStatus { get; set; } = string.Empty;
        public string NewStatus { get; set; } = string.Empty;
        public DateTime ChangedAt { get; set; } = DateTime.Now;
        public string? CustomerEmail { get; set; }
    }
}