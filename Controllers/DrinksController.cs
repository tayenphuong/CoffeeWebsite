using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebBanNuocMVC.Data;

namespace WebBanNuocMVC.Controllers
{
    //[Authorize(Roles = "Admin")]
    public class DrinksController : Controller
    {
        private readonly CoffeeShopDbContext _context;

        public DrinksController(CoffeeShopDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var coffeeShopDbContext = _context.Drinks.Include(d => d.Category);
            return View(await coffeeShopDbContext.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
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

            return View(drink);
        }

        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DrinkId,DrinkName,CategoryId,Price,Description")] Drink drink, IFormFile? imageFile)
        {
            ModelState.Remove("Image");
            
            if (imageFile != null && imageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "products");
                Directory.CreateDirectory(uploadsFolder);
                
                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }
                
                drink.Image = "~/img/products/" + uniqueFileName;
            }
            else
            {
                drink.Image = "~/img/menu-1.jpg";
            }
            
            if (ModelState.IsValid)
            {
                _context.Add(drink);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Product created successfully!";
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName", drink.CategoryId);
            return View(drink);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var drink = await _context.Drinks.FindAsync(id);
            if (drink == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName", drink.CategoryId);
            return View(drink);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("DrinkId,DrinkName,CategoryId,Price,Description,Image")] Drink drink, IFormFile? imageFile)
        {
            if (id != drink.DrinkId)
            {
                return NotFound();
            }

            if (imageFile != null && imageFile.Length > 0)
            {
                ModelState.Remove("Image");
                
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "products");
                Directory.CreateDirectory(uploadsFolder);
                
                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }
                
                drink.Image = "~/img/products/" + uniqueFileName;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(drink);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Product updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DrinkExists(drink.DrinkId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName", drink.CategoryId);
            return View(drink);
        }

        public async Task<IActionResult> Delete(int? id)
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

            return View(drink);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var drink = await _context.Drinks.FindAsync(id);
            if (drink != null)
            {
                _context.Drinks.Remove(drink);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DrinkExists(int id)
        {
            return _context.Drinks.Any(e => e.DrinkId == id);
        }
    }
}
