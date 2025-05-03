using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReadHaven.Models.Enums;
using ReadHaven.Models.Order;
using ReadHaven.Models;
using ReadHaven.Services;
using ReadHaven.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using ReadHaven.Models.User;
using Microsoft.EntityFrameworkCore;
using MailKit.Search;

namespace ReadHaven.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class BookOrderController : BaseController
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
           var hasCartItems = _context.CartItems.Any(c => c.UserId == UserId && !c.IsDeleted);
           if (!hasCartItems)
           {
              return RedirectToAction("Index", "Book");
           }
           return View();
        }

        [HttpGet("UserProductDetails")]
        public IActionResult UserProductDetails()
        {
            var cartItems = _cartService.GetOrderCartItemsForUser(UserId);

            int totalQuantity = 0;
            decimal totalAmount = 0;

            foreach (var item in cartItems)
            {
                totalQuantity += item.Quantity;
                totalAmount += item.Quantity * item.Price;
            }

            return Ok(new
            {
                totalQuantity,
                totalAmount,
                Discount = 10.00,
                Tax = 5.00
            });
        }

        [HttpGet("GetMyOrders")]
        public IActionResult GetMyOrders()
        {
            var orders = _context.Orders
                .Where(o => o.UserId == UserId)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new
                {
                    o.Id,
                    TotalAmount = _context.PaymentTransactions
                        .Where(p => p.OrderId == o.Id)
                        .Select(p => p.TotalAmount)
                        .FirstOrDefault(),
                    o.Status,
                    o.OrderDate
                })
                .ToList();

            return Ok(orders);
        }

        [HttpGet("GetMyOrdersPending")]
        public IActionResult GetMyOrdersPending()
        {
            var orders = _context.Orders
                .Where(o => o.UserId == UserId && o.Status == OrderStatus.Pending)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new
                {
                    o.Id,
                    TotalAmount = _context.PaymentTransactions
                        .Where(p => p.OrderId == o.Id)
                       .Select(p => p.TotalAmount)
                       .FirstOrDefault(),
                    o.Status,
                    o.OrderDate
                })
                .ToList();

            return Ok(orders);
        }


        [Authorize(Roles = "Admin")]
        [HttpGet("GetAllUserOrders")]
        public IActionResult GetAllUserOrders()
        {
            var orders = _context.Orders
                .OrderBy(o => o.Status)
                .ThenByDescending(o => o.OrderDate)
                .Select(o => new
                {
                    o.Id,
                    TotalAmount = _context.PaymentTransactions
                        .Where(p => p.OrderId == o.Id)
                        .Select(p => p.TotalAmount)
                        .FirstOrDefault(),
                    o.Status,
                    o.OrderDate
                })
                .ToList();

            return Ok(orders);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("ChangeOrderStatus")]
        public IActionResult ChangeOrderStatus(Guid orderId, OrderStatus status)
        {
            var order = _context.Orders.FirstOrDefault(o => o.Id == orderId);

            if (order == null)
                return NotFound();

            order.Status = status;
            _context.Orders.Update(order);
            _context.SaveChanges();

            return Ok();
        }

        [HttpPost("ConfirmOrder")]
        public IActionResult ConfirmOrder([FromBody] OrderViewModel order)
        {
            if (!ModelState.IsValid)
            {
                return Ok(new { success = false, message = "Invalid input. Please check the form and try again." });
            }

            var existingOrder = _context.Orders.FirstOrDefault(o => o.Id == order.Id && o.UserId == UserId);

            if (existingOrder != null)
            {
                existingOrder.ShippingAddress = order.ShippingAddress;
                existingOrder.ShippingCity = order.ShippingCity;
                existingOrder.ShippingPostalCode = order.ShippingPostalCode;
                existingOrder.ShippingCountry = order.ShippingCountry;
                existingOrder.ShippingContact = order.ShippingContact;

                _context.Orders.Update(existingOrder);
                _context.SaveChanges();

                return Ok(new { success = true, orderId = existingOrder.Id, message = "Shipping info updated." });
            }

            var newOrder = new Order
            {
                UserId = UserId,
                ShippingAddress = order.ShippingAddress,
                ShippingCity = order.ShippingCity,
                ShippingPostalCode = order.ShippingPostalCode,
                ShippingCountry = order.ShippingCountry,
                ShippingContact = order.ShippingContact,
                PossibleDayToShip = DateTime.UtcNow.AddDays(5),
            };

            _context.Orders.Add(newOrder);
            _context.SaveChanges();

            return Ok(new { success = true, orderId = newOrder.Id, message = "New order created." });
        }

        [HttpPost("OrderWithPreviousAddress")]
        public IActionResult ConfirmOrderWithPreviousAddress(Guid orderId)
        {
            var existingOrder = _context.Orders.FirstOrDefault(o => o.Id == orderId && o.UserId == UserId);

            if (existingOrder != null)
            {
                _context.Orders.Update(existingOrder);
                _context.SaveChanges();
                return Ok(new { success = true, orderId = existingOrder.Id, message = "Shipping info updated." });
            }

            existingOrder = _context.Orders
                   .Where(o => o.UserId == UserId)
                   .OrderByDescending(o => o.UpdatedAt)
                   .FirstOrDefault();

            if (existingOrder == null)
            {
                return Ok(new { success = false, message = "No previous Shipping Address found." });
            }

            var newOrder = new Order
            {
                UserId = UserId,
                ShippingAddress = existingOrder.ShippingAddress,
                ShippingCity = existingOrder.ShippingCity,
                ShippingPostalCode = existingOrder.ShippingPostalCode,
                ShippingCountry = existingOrder.ShippingCountry,
                ShippingContact = existingOrder.ShippingContact,
                PossibleDayToShip = DateTime.UtcNow.AddDays(5),
            };

            _context.Orders.Add(newOrder);
            _context.SaveChanges();

            return Ok(new { success = true, orderId = newOrder.Id, message = "New order created." });
        }
    }
}
