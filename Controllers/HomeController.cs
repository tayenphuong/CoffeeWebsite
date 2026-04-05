using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanNuocMVC.Data;
using WebBanNuocMVC.Models;

namespace WebBanNuocMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly CoffeeShopDbContext _context;

        public HomeController(ILogger<HomeController> logger, CoffeeShopDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var featuredProducts = await _context.Drinks
                .Include(d => d.Category)
                .OrderByDescending(d => d.DrinkId)
                .Take(8)
                .ToListAsync();

            ViewBag.FeaturedProducts = featuredProducts;
            ViewBag.Categories = await _context.Categories.ToListAsync();
            ViewBag.TotalProducts = await _context.Drinks.CountAsync();
            ViewBag.TotalCategories = await _context.Categories.CountAsync();
            ViewBag.AveragePrice = await _context.Drinks.AnyAsync()
                ? await _context.Drinks.AverageAsync(d => d.Price ?? 0)
                : 0;
            ViewBag.LatestProducts = featuredProducts.Take(3).ToList();

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
