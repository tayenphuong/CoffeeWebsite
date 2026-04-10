using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using WebBanNuocMVC.Data;

namespace WebBanNuocMVC.DesignPatterns.Chain
{
    public class DrinkAvailabilityHandler : CheckoutHandlerBase
    {
        private readonly CoffeeShopDbContext _context;

        public DrinkAvailabilityHandler(CoffeeShopDbContext context)
        {
            _context = context;
        }

        public override async Task<CheckoutChainResult> HandleAsync(CheckoutProcessingContext context)
        {
            var drinkIds = context.Request.CartItems
                .Select(x => x.DrinkId)
                .Distinct()
                .ToList();

            var drinks = await _context.Drinks
                .Where(d => drinkIds.Contains(d.DrinkId))
                .Select(d => new
                {
                    d.DrinkId,
                    Price = d.Price ?? 0m
                })
                .ToListAsync();

            if (drinks.Count() != drinkIds.Count())
            {
                return CheckoutChainResult.Fail("Có sản phẩm không còn tồn tại trong hệ thống.");
            }

            foreach (var cartItem in context.Request.CartItems)
            {
                var drink = drinks.FirstOrDefault(x => x.DrinkId == cartItem.DrinkId);

                if (drink == null)
                {
                    return CheckoutChainResult.Fail("Có sản phẩm không hợp lệ trong giỏ hàng.");
                }

                context.CurrentPrices[cartItem.DrinkId] = drink.Price;
            }

            return await HandleNextAsync(context);
        }
    }
}