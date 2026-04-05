using System.Collections.Generic;

namespace WebBanNuocMVC.DesignPatterns.Proxy
{
    public class AdminDashboardData
    {
        public int TotalDrinks { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalOrders { get; set; }
        public int TotalAccounts { get; set; }

        public decimal RevenueToday { get; set; }
        public decimal RevenueThisMonth { get; set; }
        public decimal RevenueThisYear { get; set; }

        public List<MonthlyRevenueItem> MonthlyRevenue { get; set; } = new();
    }

    public class MonthlyRevenueItem
    {
        public string MonthLabel { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
    }
}