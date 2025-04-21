using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReadHaven.Models.Enums;
using ReadHaven.Models.Order;
using ReadHaven.Models.User;
using ReadHaven.Services;
using System.Security.Claims;

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

        [HttpGet("GetMyOrders")]
        public IActionResult GetMyOrders()
        {
            Guid userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var orders = _context.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new
                {
                    o.Id,
                    TotalAmount = o.TotalAmount,
                    o.Status,
                    OrderDate = o.OrderDate
                })
                .ToList();

            return Ok(orders);
        }

        [HttpGet("GetMyOrdersPending")]
        public IActionResult GetMyOrdersPending()
        {
            Guid userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var orders = _context.Orders
                .Where(o => o.UserId == userId && o.Status == OrderStatus.Pending)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new
                {
                    o.Id,
                    TotalAmount = o.TotalAmount,
                    o.Status,
                    OrderDate = o.OrderDate
                })
                .ToList();

            return Ok(orders);
        }


        [Authorize(Roles = "Admin")]
        [HttpGet("GetAllUserOrders")]
        public IActionResult GetUserOrders()
        {
            var orders = _context.Orders
                         .OrderBy(o => o.Status)
                         .ThenByDescending(o => o.OrderDate)
                         .ToList();

            return Ok(orders);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("ChangeOrderStatus")]
        public IActionResult ChangeOrderStatus(Guid orderId,OrderStatus status)
        {
            var order = _context.Orders
                .FirstOrDefault(o => o.Id == orderId);

            order.Status = status;
            _context.Orders.Update(order);
            _context.SaveChanges();

            return Ok();
        }

        [HttpPost("ConfirmOrder")]
        public IActionResult ConfirmOrder(Guid orderId)
        {
            Guid userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var order = _context.Orders
                .FirstOrDefault(o => o.Id == orderId && o.UserId == userId);

            if (order == null)
                return NotFound();

            order.Status = OrderStatus.Processing;   

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
                OrderDate = DateTime.UtcNow
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
    }
}
