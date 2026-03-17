using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanNuocMVC.Data;

namespace WebBanNuocMVC.Controllers
{
    //[Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly CoffeeShopDbContext _context;

        public AdminController(CoffeeShopDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {
            ViewBag.TotalDrinks = await _context.Drinks.CountAsync();
            ViewBag.TotalCategories = await _context.Categories.CountAsync();
            ViewBag.TotalOrders = await _context.Orders.CountAsync();
            ViewBag.TotalCustomers = await _context.Customers.CountAsync();

            ViewBag.RecentOrders = await _context.Orders
                .Include(o => o.Customer)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToListAsync();

            ViewBag.TotalRevenue = await _context.Orders
                .Where(o => o.Status == "Completed" || o.Status == "Paid")
                .SumAsync(o => o.TotalAmount ?? 0);

            return View();
        }
    }
}
