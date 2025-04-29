using ReadHaven.Models.Enums;
using ReadHaven.Models;

namespace ReadHaven.ViewModels
{
    public class PaymentViewModel
    {
        public Guid OrderId { get; set; }
        public string Email { get; set; }
        public string Otp { get; set; }
        public decimal TotalAmount { get; set; }
        public Currency Currency { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public decimal DiscountAmount { get; set; } = 5;
        public decimal TaxAmount { get; set; } = 10;

    }
}
