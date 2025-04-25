using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReadHaven.Services;
using ReadHaven.Models.Enums;
using System;
using ReadHaven.ViewModels;

namespace ReadHaven.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class PaymentController : BaseController
    {
        private readonly AppDbContext _context;
        private readonly IEmailSender _emailSender;

        public PaymentController(AppDbContext context, IEmailSender emailSender)
        {
            _context = context;
            _emailSender = emailSender;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("LoadPayment")]
        public IActionResult LoadPayment(Guid orderId)
        {
            var order = _context.Orders.Find(orderId);

            if (order == null || order.Status != OrderStatus.Pending)
            {
                return NotFound();
            }else
            {
                var model = new OrderViewModel
                {
                    Id = order.Id,
                    Amount = order.TotalAmount
                };
                return PartialView("_Payment", model);
            }
        }


    }
}
