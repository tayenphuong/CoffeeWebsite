using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebBanNuocMVC.DesignPatterns.Singleton;
using WebBanNuocMVC.Data;
using WebBanNuocMVC.Models.ViewModels;

namespace WebBanNuocMVC.Controllers
{
    public class AuthController : Controller
    {
        private readonly CoffeeShopDbContext _context;
        private readonly ILoggerService _logger;

        public AuthController(CoffeeShopDbContext context, ILoggerService logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null, string? returnToAdmin = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.ReturnToAdmin = returnToAdmin;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null, string? returnToAdmin = null)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ReturnUrl = returnUrl;
                ViewBag.ReturnToAdmin = returnToAdmin;
                return View(model);
            }

            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Username == model.Username);

            if (account == null || !VerifyPassword(model.Password, account.Password))
            {
                ViewBag.ReturnUrl = returnUrl;
                ViewBag.ReturnToAdmin = returnToAdmin;
                ModelState.AddModelError("", "Invalid username or password");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, account.Username ?? ""),
                new Claim(ClaimTypes.NameIdentifier, account.AccountId.ToString()),
                new Claim(ClaimTypes.Role, account.Role ?? "Customer")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(24)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            HttpContext.Session.SetInt32("UserId", account.AccountId);
            HttpContext.Session.SetString("Username", account.Username ?? "");
            HttpContext.Session.SetString("Role", account.Role ?? "Customer");

            TempData["Success"] = "Login successful!";

            // 1. Ghi log thành công
            _logger.LogInfo($"LOGIN | ID:{account.AccountId} | USER:{account.Username} | ROLE:{account.Role}");

            // 2. ƯU TIÊN 1: Nếu là Admin và muốn vào trang Admin
            if (string.Equals(account.Role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                // Nếu có returnUrl chỉ định trang admin cụ thể thì về đó, không thì về Dashboard
                if (!string.IsNullOrWhiteSpace(returnUrl) && returnUrl.Contains("Admin", StringComparison.OrdinalIgnoreCase))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Dashboard", "Admin");
            }

            // 3. ƯU TIÊN 2: Nếu có returnUrl (thường dành cho Customer muốn quay lại trang đang xem dở)
            if (!string.IsNullOrWhiteSpace(returnUrl))
            {
                return Redirect(returnUrl);
            }

            // 4. MẶC ĐỊNH: Về trang chủ
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (await _context.Accounts.AnyAsync(a => a.Username == model.Username))
            {
                ModelState.AddModelError("Username", "Username already exists");
                return View(model);
            }

            var account = new Account
            {
                Username = model.Username,
                Password = HashPassword(model.Password),
                Role = "Customer"
            };

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            var customer = new Customer
            {
                CustomerName = model.FullName ?? model.Username,
                Email = model.Email,
                Phone = model.Phone
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Registration successful! Please login.";
            return RedirectToAction("Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            TempData["Success"] = "You have been logged out.";
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private bool VerifyPassword(string password, string? hashedPassword)
        {
            if (string.IsNullOrEmpty(hashedPassword))
                return false;

            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
            }
            catch
            {
                return false;
            }
        }
    }
}
