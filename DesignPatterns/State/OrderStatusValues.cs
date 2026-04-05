namespace WebBanNuocMVC.DesignPatterns.State
{
    public static class OrderStatusValues
    {
        public const string Pending = "Pending";
        public const string PendingPayment = "Pending Payment";
        public const string Paid = "Paid";
        public const string Preparing = "Preparing";
        public const string Completed = "Completed";
        public const string Cancelled = "Cancelled";
    }
}