using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using ReadHaven.Services;
using ReadHaven.Models.Order;


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
        public async Task<IActionResult> Index()
        {
            Guid userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            PlaceOrder(userId);
            return View();
        }

        [HttpGet("GetUserOrder")]
        public async Task<IActionResult> GetUserOrderAsync()
        {
            Guid userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var order = _context.Orders
                .Where(o => o.UserId == userId)
                .FirstOrDefault();

            if (order == null)
            {
                return Json(new { TotalAmount = 0.00, Status = "No Order", OrderDate = DateTime.UtcNow });
            }

            var orderData = new
            {
                TotalAmount = order.TotalAmount.ToString("0.00"),
                Status = order.Status,
                OrderDate = order.OrderDate
            };

            return Ok(orderData);
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
                OrderDate = DateTime.UtcNow
            };

            _context.Orders.Add(order);

            foreach(var item in cartItems)
            {
                item.OrderId = order.Id;
                item.IsDeleted = true;
            }   
            _context.CartItems.UpdateRange(cartItems);
            _context.SaveChanges();
        }

    }
}
