using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanNuocMVC.Data;

namespace WebBanNuocMVC.Controllers
{
    public class ShopController : Controller
    {
        private readonly CoffeeShopDbContext _context;

        public ShopController(CoffeeShopDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? categoryId, string? search, int page = 1)
        {
            const int pageSize = 12;

            var query = _context.Drinks.Include(d => d.Category).AsQueryable();

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(d => d.CategoryId == categoryId.Value);
                ViewBag.CurrentCategory = categoryId.Value;
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(d => d.DrinkName!.Contains(search) ||
                                         d.Description!.Contains(search));
                ViewBag.SearchTerm = search;
            }

            var totalItems = await query.CountAsync();
            ViewBag.TotalItems = totalItems;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.CurrentPage = page;

            var drinks = await query
                .OrderBy(d => d.Category!.CategoryName)
                .ThenBy(d => d.DrinkName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var categories = await _context.Categories
                .OrderBy(c => c.CategoryName)
                .ToListAsync();

            ViewBag.Categories = categories;
            ViewBag.CategoryCount = categories.Count;

            return View(drinks);
        }

        public async Task<IActionResult> ProductDetail(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var drink = await _context.Drinks
                .Include(d => d.Category)
                .FirstOrDefaultAsync(m => m.DrinkId == id);

            if (drink == null)
            {
                return NotFound();
            }

            ViewBag.RelatedProducts = await _context.Drinks
                .Where(d => d.CategoryId == drink.CategoryId && d.DrinkId != id)
                .Take(4)
                .ToListAsync();

            return View(drink);
        }

        public async Task<IActionResult> Menu()
        {
            var drinksGroupedByCategory = await _context.Drinks
                .Include(d => d.Category)
                .OrderBy(d => d.Category!.CategoryName)
                .ThenBy(d => d.DrinkName)
                .GroupBy(d => d.Category!.CategoryName)
                .ToListAsync();

            ViewBag.TotalItems = drinksGroupedByCategory.Sum(g => g.Count());
            ViewBag.TotalGroups = drinksGroupedByCategory.Count;

            return View(drinksGroupedByCategory);
        }
    }
}
