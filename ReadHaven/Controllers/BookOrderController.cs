using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using ReadHaven.Services;

namespace ReadHaven.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class BookOrderController : Controller
    {
        private readonly AppDbContext _context;
        public BookOrderController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Get cart items for the user (merge if needed)
            var cartItems = await _cartService.GetCartItemsAsync(userId);

            if (cartItems == null || !cartItems.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Index", "Cart");
            }

            // Create new Order
            var order = new Order
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                OrderDate = DateTime.Now,
                Status = "Placed",
                OrderItems = cartItems.Select(ci => new OrderItem
                {
                    Id = Guid.NewGuid(),
                    BookId = ci.BookId,
                    Quantity = ci.Quantity,
                    Price = ci.Book.Price
                }).ToList()
            };

            await _orderRepository.CreateAsync(order);
            await _cartService.ClearCartAsync(userId);

            return RedirectToAction("OrderDetails", new { id = order.Id });
        }

    }
}
