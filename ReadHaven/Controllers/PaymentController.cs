using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReadHaven.Services;
using ReadHaven.Models.Enums;
using System;
using ReadHaven.ViewModels;
using Payment.Models;
using ReadHaven.Models;
using ReadHaven.Models.Order;

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

        [HttpPost("SendOtp")]    
        public async Task<IActionResult> SendOtp(string email)
        {
            if (IsValidEmail(email) == false)
            {
                return Ok(new { success = false, message = "Invalid email format." });
            }
            var random = new Random();
            var generatedOtp = random.Next(100000, 999999).ToString();
            var otpExpiry = DateTime.Now.AddMinutes(5);

            var otpRequest = new OtpRequest
            {
                Email = email,
                Otp = generatedOtp,
                ExpiryTime = otpExpiry,
                IsValidated = false
            };

            _context.OtpRequests.Add(otpRequest);
            _context.SaveChanges();

            string emailBody = $"Your OTP code is <b>{generatedOtp}</b>. It is valid for 5 minutes.";
            await _emailSender.SendEmailAsync(email, "Your OTP Code", emailBody);

            return Ok(new { success = true, message = "Email sent successfully." });
        }

        [HttpPost("VerifyOtp")]
        public IActionResult VerifyOtp([FromBody] PaymentViewModel payment)
        {
            var otpRequest = _context.OtpRequests
             .Where(x => x.Email == payment.Email && x.Otp == payment.Otp)
             .OrderByDescending(x => x.ExpiryTime)
             .FirstOrDefault();

            var cartItems = _context.CartItems.Where(c => c.UserId == UserId).ToList();
            if (!cartItems.Any() || otpRequest == null)
                return Ok(false);

            otpRequest.IsValidated = true;
            _context.OtpRequests.Update(otpRequest);

            foreach (var item in cartItems)
            {
                item.OrderId = payment.OrderId;
                item.IsDeleted = true;
            }

            _context.CartItems.UpdateRange(cartItems);

            var newPayment = new PaymentTransaction
            {
                OrderId = payment.OrderId,
                TotalAmount = payment.TotalAmount,
                Currency = payment.Currency,
                PaymentMethod = payment.PaymentMethod,
                Status = Status.Success
            };

            _context.PaymentTransactions.Add(newPayment);
            _context.SaveChanges();
            return Ok(true);
        }
        private bool IsValidEmail(string email)
        {
            try
            {
                var mailAddress = new System.Net.Mail.MailAddress(email);
                return mailAddress.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
