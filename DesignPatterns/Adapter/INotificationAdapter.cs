namespace WebBanNuocMVC.DesignPatterns.Adapter
{
    public interface INotificationAdapter
    {
        Task SendAsync(string to, string subject, string body);
    }
}
