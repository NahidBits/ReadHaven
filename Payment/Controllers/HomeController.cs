using System;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using MailKit.Net.Smtp;
using System.Net.Mail;
using Payment.Data;
using Payment.Models;

namespace Payment.Controllers
{
    public class HomeController : Controller
    {
        private static string _generatedOtp;
        private static DateTime _otpExpiry;

        private readonly ApplicationDbContext _context;
        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("sendEmail")]
        public IActionResult SendOtp(string email)
        {
            // Validate email format
            if (!IsValidEmail(email))
            {
                ViewBag.Message = "Invalid email format. Please enter a valid email.";
                return View("Index");
            }

            // Generate a 6-digit OTP
            var random = new Random();
            _generatedOtp = random.Next(100000, 999999).ToString();
            _otpExpiry = DateTime.Now.AddMinutes(5); // OTP valid for 5 minutes

            // Send OTP via email
            var message = new MimeMessage();

            message.From.Add(new MailboxAddress("Your App", "mdnahidchy2020@gmail.com"));
            message.To.Add(new MailboxAddress("", email));
            message.Subject = "Your OTP Code";
            message.Body = new TextPart("plain")
            {
                Text = $"Your OTP code is {_generatedOtp}. It is valid for 5 minutes."
            };

            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                client.Connect("smtp.gmail.com", 587, false);
                client.Authenticate("mdnahidchy2020@gmail.com", "dxeh kmly kxpq pxjj");
                client.Send(message);
                client.Disconnect(true);
            }

            var otpRequest = new OtpRequest
            {
                Email = email,
                Otp = _generatedOtp,
                ExpiryTime = _otpExpiry,
                IsValidated = false
            };  

            _context.OtpRequests.Add(otpRequest);
            _context.SaveChanges(); 

            ViewBag.Message = "OTP sent successfully!";
            return Ok(new {success = true});
        }

        [HttpPost("ValidateOtp")]
        public IActionResult ValidateOtp(string email,string otp)
        {

            var otpRequest = _context.OtpRequests
             .Where(x => x.Email == email && x.Otp == otp)
             .OrderByDescending(x => x.ExpiryTime)
             .FirstOrDefault();

            if(otpRequest != null)
            {
                ViewBag.Message = "OTP has expired. Please request a new one.";
                otpRequest.IsValidated = true;
            }
            else
            {
                ViewBag.Message = "Invalid OTP. Please try again.";
                return Ok(new { success = false });
            }

            _context.OtpRequests.Update(otpRequest);
            _context.SaveChanges();
            return Ok(new { success = true });  
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

        public IActionResult StorePayment(decimal amount, int currency)
        {
            // Create a new payment record
            var payment = new PaymentModel
            {
                Amount = amount,
                Currency = (Currency)currency,
                PaymentMethod = PaymentMethod.Email,
                Status = Status.Success 
            };

            // Save to the database
            _context.Payments.Add(payment);
            _context.SaveChanges();

            return Json(new { success = true, message = "Payment stored successfully!" });
        }
    }
}
