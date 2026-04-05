namespace WebBanNuocMVC.Helpers
{
    public static class OrderStatusMapper
    {
        public static string NormalizeStatus(string? status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return "pending";

            status = status.Trim().ToLower();

            return status switch
            {
                "pendingpayment" => "pendingpayment",
                "completed" => "completed",
                "cancelled" => "cancelled",
                "paid" => "completed",
                "pending" => "pending",
                _ => "pending"
            };
        }

        public static string GetDisplayText(string? status)
        {
            var normalized = NormalizeStatus(status);

            return normalized switch
            {
                "pendingpayment" => "CHỜ THANH TOÁN",
                "completed" => "HOÀN TẤT",
                "cancelled" => "ĐÃ HỦY",
                _ => "ĐANG CHỜ"
            };
        }

        public static string GetBadgeClass(string? status)
        {
            var normalized = NormalizeStatus(status);

            return normalized switch
            {
                "pendingpayment" => "badge bg-secondary px-3 py-2 rounded-pill",
                "completed" => "badge bg-success px-3 py-2 rounded-pill",
                "cancelled" => "badge bg-danger px-3 py-2 rounded-pill",
                _ => "badge bg-warning text-dark px-3 py-2 rounded-pill"
            };
        }
    }
}