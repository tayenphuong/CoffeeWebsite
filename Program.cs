using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using WebBanNuocMVC.DesignPatterns.Observer;
using WebBanNuocMVC.DesignPatterns.Singleton;
using WebBanNuocMVC.Data;
using WebBanNuocMVC.DesignPatterns.Builder;
using WebBanNuocMVC.DesignPatterns.Facade;
using WebBanNuocMVC.DesignPatterns.FactoryMethod;
using WebBanNuocMVC.DesignPatterns.FactoryMethod.ConcreteFactories;
using WebBanNuocMVC.DesignPatterns.Command;
using WebBanNuocMVC.DesignPatterns.Adapter;

namespace WebBanNuocMVC
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();

            builder.Services.AddDbContext<CoffeeShopDbContext>(option => {
                option.UseSqlServer(builder.Configuration.GetConnectionString("CoffeeShopMVC"));
            });

            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });



           

            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Auth/Login";
                    options.LogoutPath = "/Auth/Logout";
                    options.AccessDeniedPath = "/Auth/AccessDenied";
                    options.ExpireTimeSpan = TimeSpan.FromHours(24);
                    options.SlidingExpiration = true;
                });

            builder.Services.AddAuthorization();

            // Singleton
            builder.Services.AddSingleton<ILoggerService, LoggerService>();

            // Factory Method
            builder.Services.AddScoped<IPaymentFactory, PaymentFactory>();
            builder.Services.AddHttpClient();
            builder.Services.AddScoped<VNPayService>();
            builder.Services.AddScoped<PayPalService>();
            builder.Services.AddScoped<CODService>();

            // Builder
            builder.Services.AddTransient<IOrderBuilder, OrderBuilder>();
            builder.Services.AddScoped<OrderDirector>();

            // Facade
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<ICheckoutFacade, CheckoutFacade>();

            // Observer
            builder.Services.AddScoped<IOrderObserver, OrderLoggerObserver>();
            builder.Services.AddScoped<IOrderObserver, OrderAdminNotificationObserver>();
            builder.Services.AddScoped<IOrderSubject, OrderSubject>();

            // Command
            // ??i t? Scoped sang Singleton
            builder.Services.AddSingleton<OrderCommandInvoker>();

            // Adapter
            // ??ng ký c? hai Adapter cho cùng m?t Interface
            builder.Services.AddScoped<INotificationAdapter, EmailNotificationAdapter>();
            builder.Services.AddScoped<INotificationAdapter, TelegramNotificationAdapter>();

            // ??ng ký các thành ph?n h? tr?
            builder.Services.AddHttpClient(); // Cho Telegram
            builder.Services.AddScoped<IOrderObserver, CustomerNotificationObserver>();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<CoffeeShopDbContext>();
                DbInitializer.Initialize(context);
            }

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
