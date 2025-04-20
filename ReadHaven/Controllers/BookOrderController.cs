using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReadHaven.Models.Order;
using ReadHaven.Services;
using System.Security.Claims;
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
        private readonly CartService _cartService;

        public BookOrderController(AppDbContext context, CartService cartService)
        {
            _context = context;
            _cartService = cartService;
        }

        [HttpGet("Index")]
        public IActionResult Index()
        {
            Guid userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            PlaceOrder(userId);
            return View();
        }

        [HttpGet("GetUserOrders")]
        public IActionResult GetUserOrders()
        {
            Guid userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var orders = _context.Orders
                .Where(o => o.UserId == userId && o.Status == "Pending")
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new
                {
                    o.Id,
                    TotalAmount = o.TotalAmount.ToString("0.00"),
                    o.Status,
                    OrderDate = o.OrderDate.ToString("yyyy-MM-dd HH:mm:ss")
                })
                .ToList();

            return Ok(orders);
        }

        [HttpPost("DeleteOrder")]
        public IActionResult DeleteOrder(Guid orderId)
        {
            Guid userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var order = _context.Orders
                .FirstOrDefault(o => o.Id == orderId && o.UserId == userId);

            if (order == null)
                return NotFound();

            order.Status = "Done";               
            order.UpdatedAt = DateTime.UtcNow;

            _context.Orders.Update(order);
            _context.SaveChanges();

            return Ok();
        }


        public void PlaceOrder(Guid userId)
        {
            var cartItems = _context.CartItems.Where(c => c.UserId == userId).ToList();

            if (cartItems == null || !cartItems.Any())
                return;

            var order = new Order
            {
                UserId = userId,
                TotalAmount = cartItems.Sum(c => c.Price * c.Quantity),
                OrderDate = DateTime.UtcNow,
                Status = "Pending"
            };

            _context.Orders.Add(order);

            foreach (var item in cartItems)
            {
                item.OrderId = order.Id;
                item.IsDeleted = true;
            }

            _context.CartItems.UpdateRange(cartItems);
            _context.SaveChanges();
        }
        
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
