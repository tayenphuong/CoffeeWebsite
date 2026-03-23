namespace WebBanNuocMVC.DesignPatterns.Command
{
    public interface IOrderCommand
    {
        string Description { get; } // Để hiển thị: "Đã hoàn tác: Hủy đơn hàng #12"
        Task ExecuteAsync();
        Task UndoAsync();
    }
}
