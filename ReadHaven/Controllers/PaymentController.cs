using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReadHaven.Services;
using ReadHaven.Models.Enums;
using System;
using ReadHaven.ViewModels;
using ReadHaven.Models.Payment;
using ReadHaven.Models;
using ReadHaven.Models.Order;
using jsreport.Types;
using Microsoft.EntityFrameworkCore;
using jsreport.AspNetCore;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using ReadHaven.Models.User;


namespace ReadHaven.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class PaymentController : BaseController
    {
        private readonly AppDbContext _context;
        private readonly IEmailSender _emailSender;
        private readonly IJsReportMVCService _jsReportMVCService;
        private readonly ICompositeViewEngine _viewEngine;

        public PaymentController(AppDbContext context, IEmailSender emailSender, IJsReportMVCService jsReportMVCService, ICompositeViewEngine viewEngine)
        {
            _context = context;
            _emailSender = emailSender;
            _jsReportMVCService = jsReportMVCService;
            _viewEngine = viewEngine;
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
        public async Task<IActionResult> VerifyOtp([FromBody] PaymentViewModel payment)
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
            }

            _context.CartItems.UpdateRange(cartItems);

            var newPayment = new PaymentTransaction
            {
                OrderId = payment.OrderId,
                TotalAmount = cartItems.Sum(c => c.Price*c.Quantity),
                Currency = payment.Currency,
                PaymentMethod = payment.PaymentMethod,
                Status = Status.Success
            };


            _context.PaymentTransactions.Add(newPayment);
            _context.SaveChanges();

            // Generate PDF
            var pdfBytes = await GenerateOrderSummary(payment.OrderId);

            // Send email with PDF
            var subject = "Order Confirmation with Summary";
            var message = "Thank you for your order. Please find the attached order summary.";
            var attachments = new List<(byte[] Content, string FileName, string MimeType)>
    {
        (pdfBytes, "OrderSummary.pdf", "application/pdf")
    };

            await _emailSender.SendEmailWithAttachmentAsync(payment.Email, subject, message, attachments);


            _context.CartItems.RemoveRange(cartItems);
            _context.OtpRequests.Remove(otpRequest);
            await _context.SaveChangesAsync();
            return Ok(true);
        }

        public async Task<byte[]> GenerateOrderSummary(Guid orderId)
        {
            var order = _context.Orders.FirstOrDefault(o => o.Id == orderId);
            var cartItems = _context.CartItems.Where(c => c.OrderId == orderId).ToList();
            var paymentTransaction = _context.PaymentTransactions.FirstOrDefault(p => p.OrderId == orderId);

            var bookIds = cartItems.Select(c => c.BookId).Distinct().ToList();
            var books = _context.Books
                .Where(b => bookIds.Contains(b.Id))
                .ToDictionary(b => b.Id, b => new { b.Title, b.ImagePath });

            var cartItemView = cartItems
    .Where(c => !IsAuthenticated || c.UserId == UserId)
    .Select(c =>
    {
        books.TryGetValue(c.BookId, out var bookInfo);

        string imagePath = !string.IsNullOrEmpty(bookInfo?.ImagePath)
            ? bookInfo.ImagePath
            : "/uploads/book/Default_image.webp";

        string base64Image = string.Empty;

        try
        {
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", imagePath.TrimStart('/'));
            if (System.IO.File.Exists(fullPath))
            {
                byte[] imageBytes = System.IO.File.ReadAllBytes(fullPath);
                string base64 = Convert.ToBase64String(imageBytes);

                string extension = Path.GetExtension(fullPath).ToLower();
                string mimeType = extension switch
                {
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    ".webp" => "image/webp",
                    _ => "image/png"
                };

                base64Image = $"data:{mimeType};base64,{base64}";
            }
        }
        catch
        {
            base64Image = string.Empty; // fallback or log
        }

        return new CartItemViewModel
        {
            Id = c.Id,
            BookId = c.BookId,
            BookTitle = bookInfo?.Title ?? "Unknown",
            Base64Image = base64Image,
            Quantity = c.Quantity,
            UnitPrice = c.Price
        };
    })
    .ToList();


            var viewModel = new Models.Payment.Report
            {
                Order = order,
                Items = cartItemView,
                Payment = paymentTransaction
            };

            var htmlContent = await RenderViewToStringAsync("OrderSummary", viewModel);
            var routeData = this.HttpContext.GetRouteData();

            var report = await _jsReportMVCService.RenderViewAsync(
                HttpContext,
                new RenderRequest
                {
                    Template = new Template
                    {
                        Recipe = Recipe.ChromePdf,
                        Engine = Engine.None,
                        Content = htmlContent
                    },
                    Data = viewModel
                },
                routeData,
                "Views/Payment/OrderSummary.cshtml",
                viewModel
            );

            using var memoryStream = new MemoryStream();
            await report.Content.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }


        private async Task<string> RenderViewToStringAsync(string viewName, object model)
        {
            var controllerContext = this.ControllerContext;

            var viewResult = _viewEngine.FindView(controllerContext, viewName, isMainPage: false);
            if (!viewResult.Success)
            {
                throw new InvalidOperationException($"View '{viewName}' not found.");
            }

            var viewDataDictionary = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                Model = model
            };

            var tempDataProvider = HttpContext.RequestServices.GetService(typeof(ITempDataProvider)) as ITempDataProvider;
            var tempDataDictionary = new TempDataDictionary(HttpContext, tempDataProvider);

            using var sw = new StringWriter();
            var viewContext = new ViewContext(
                controllerContext,
                viewResult.View,
                viewDataDictionary,
                tempDataDictionary,
                sw,
                new HtmlHelperOptions()
            );

            await viewResult.View.RenderAsync(viewContext);
            return sw.ToString();
        }

    }
}
