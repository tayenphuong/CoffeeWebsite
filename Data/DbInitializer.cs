using WebBanNuocMVC.Data;


// class này tạo database ban đầu để thêm vào db ,  đừng sửa đổi gì
namespace WebBanNuocMVC.Data
{
    public static class DbInitializer
    {
        public static void Initialize(CoffeeShopDbContext context)
        {
            context.Database.EnsureCreated();

            // Setup Data first time only
            if (context.Accounts.Any())
            {
                return; // Database đã được seed rồi
            }

            // TẠO DATA MỚI (chỉ chạy lần đầu tiên)
            var accounts = new Account[]
            {
                new Account
                {
                    Username = "admin",
                    Password = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    Role = "Admin"
                },
                new Account
                {
                    Username = "customer",
                    Password = BCrypt.Net.BCrypt.HashPassword("customer123"),
                    Role = "Customer"
                }
            };
            context.Accounts.AddRange(accounts);
            context.SaveChanges();

            var categories = new Category[]
            {
                new Category { CategoryName = "Coffee" },
                new Category { CategoryName = "Tea" },
                new Category { CategoryName = "Smoothie" },
                new Category { CategoryName = "Juice" }
            };
            context.Categories.AddRange(categories);
            context.SaveChanges();

            var drinks = new Drink[]
            {
                new Drink
                {
                    DrinkName = "Cappuccino",
                    CategoryId = 1,
                    Price = 45000,
                    Description = "Rich espresso with steamed milk and foam",
                    Image = "~/img/menu-1.jpg"
                },
                new Drink
                {
                    DrinkName = "Latte",
                    CategoryId = 1,
                    Price = 40000,
                    Description = "Smooth espresso with steamed milk",
                    Image = "~/img/menu-2.jpg"
                },
                new Drink
                {
                    DrinkName = "Espresso",
                    CategoryId = 1,
                    Price = 35000,
                    Description = "Strong and bold coffee shot",
                    Image = "~/img/menu-3.jpg"
                },
                new Drink
                {
                    DrinkName = "Green Tea",
                    CategoryId = 2,
                    Price = 30000,
                    Description = "Fresh and healthy green tea",
                    Image = "~/img/menu-1.jpg"
                },
                new Drink
                {
                    DrinkName = "Mango Smoothie",
                    CategoryId = 3,
                    Price = 50000,
                    Description = "Fresh mango blended smoothie",
                    Image = "~/img/menu-2.jpg"
                },
                new Drink
                {
                    DrinkName = "Orange Juice",
                    CategoryId = 4,
                    Price = 35000,
                    Description = "Freshly squeezed orange juice",
                    Image = "~/img/menu-3.jpg"
                }
            };
            context.Drinks.AddRange(drinks);
            context.SaveChanges();

            var customer = new Customer
            {
                CustomerName = "John Doe",
                Email = "customer@example.com",
                Phone = "0123456789",
                Address = "123 Main Street, Hanoi"
            };
            context.Customers.Add(customer);
            context.SaveChanges();
        }
    }
}