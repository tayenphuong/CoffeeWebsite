using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WebBanNuocMVC.DesignPatterns.Proxy;

namespace WebBanNuocMVC.Controllers
{
    public class AdminController : Controller
    {
        private readonly IAdminDashboardSubject _dashboardSubject;

        public AdminController(IAdminDashboardSubject dashboardSubject)
        {
            _dashboardSubject = dashboardSubject;
        }

        public async Task<IActionResult> Dashboard()
        {
            var result = await _dashboardSubject.GetDashboardAsync();

            ViewBag.HasAccess = result.HasAccess;
            ViewBag.RoleLabel = result.RoleLabel;
            ViewBag.AccessMessage = result.AccessMessage;

            if (result.HasAccess && result.Data != null)
            {
                ViewBag.TotalDrinks = result.Data.TotalDrinks;
                ViewBag.TotalCustomers = result.Data.TotalCustomers;
                ViewBag.TotalOrders = result.Data.TotalOrders;
                ViewBag.TotalAccounts = result.Data.TotalAccounts;

                ViewBag.RevenueToday = result.Data.RevenueToday;
                ViewBag.RevenueThisMonth = result.Data.RevenueThisMonth;
                ViewBag.RevenueThisYear = result.Data.RevenueThisYear;
                ViewBag.MonthlyRevenue = result.Data.MonthlyRevenue;
            }

            return View();
        }
    }
}