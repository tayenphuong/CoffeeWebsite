namespace WebBanNuocMVC.DesignPatterns.Proxy
{
    public class AdminDashboardAccessResult
    {
        public bool HasAccess { get; set; }
        public string RoleLabel { get; set; } = string.Empty;

        public string AccessMessage { get; set; } = string.Empty;

        // để tương thích nếu file cũ đang dùng Message
        public string Message
        {
            get => AccessMessage;
            set => AccessMessage = value;
        }

        public AdminDashboardData? Data { get; set; }
    }
}