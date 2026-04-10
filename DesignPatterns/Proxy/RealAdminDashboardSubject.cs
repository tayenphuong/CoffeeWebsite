using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using WebBanNuocMVC.Data;

namespace WebBanNuocMVC.DesignPatterns.Proxy
{
    public class RealAdminDashboardSubject : IAdminDashboardSubject
    {
        private readonly CoffeeShopDbContext _context;

        public RealAdminDashboardSubject(CoffeeShopDbContext context)
        {
            _context = context;
        }

        public async Task<AdminDashboardAccessResult> GetDashboardAsync()
        {
            var now = DateTime.Now;
            var startToday = now.Date;
            var startMonth = new DateTime(now.Year, now.Month, 1);
            var startYear = new DateTime(now.Year, 1, 1);
            var firstMonthInRange = startMonth.AddMonths(-5);

            var orderQuery = _context.Orders.AsNoTracking();

            // Nếu hệ thống chỉ tính doanh thu cho đơn đã thanh toán / hoàn tất
            // thì tự thêm điều kiện Status đúng theo model của bạn.

            var monthlyRaw = await orderQuery
                .Where(o => o.OrderDate.HasValue && o.OrderDate.Value >= firstMonthInRange)
                .GroupBy(o => new
                {
                    Year = o.OrderDate.Value.Year,
                    Month = o.OrderDate.Value.Month
                })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Revenue = g.Sum(x => (decimal?)x.TotalAmount) ?? 0
                })
                .ToListAsync();

            var monthlyRevenue = Enumerable.Range(0, 6)
                .Select(i =>
                {
                    var month = firstMonthInRange.AddMonths(i);
                    var hit = monthlyRaw.FirstOrDefault(x => x.Year == month.Year && x.Month == month.Month);

                    return new MonthlyRevenueItem
                    {
                        MonthLabel = month.ToString("MM/yyyy"),
                        Revenue = hit?.Revenue ?? 0
                    };
                })
                .ToList();

            var data = new AdminDashboardData
            {
                TotalDrinks = await _context.Drinks.CountAsync(),
                TotalCustomers = await _context.Customers.CountAsync(),
                TotalOrders = await _context.Orders.CountAsync(),
                TotalAccounts = await _context.Accounts.CountAsync(),

                RevenueToday = await orderQuery
                    .Where(o => o.OrderDate.HasValue && o.OrderDate.Value >= startToday)
                    .SumAsync(o => (decimal?)o.TotalAmount) ?? 0,

                RevenueThisMonth = await orderQuery
                    .Where(o => o.OrderDate.HasValue && o.OrderDate.Value >= startMonth)
                    .SumAsync(o => (decimal?)o.TotalAmount) ?? 0,

                RevenueThisYear = await orderQuery
                    .Where(o => o.OrderDate.HasValue && o.OrderDate.Value >= startYear)
                    .SumAsync(o => (decimal?)o.TotalAmount) ?? 0,

                MonthlyRevenue = monthlyRevenue
            };

            return new AdminDashboardAccessResult
            {
                HasAccess = true,
                RoleLabel = "Admin",
                AccessMessage = "Truy cập hợp lệ",
                Data = data
            };
        }
    }
}