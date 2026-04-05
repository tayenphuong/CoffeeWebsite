namespace WebBanNuocMVC.DesignPatterns.State
{
    public interface IOrderState
    {
        string Name { get; }

        string Pay();
        string StartPreparing();
        string Complete();
        string Cancel();
    }
}