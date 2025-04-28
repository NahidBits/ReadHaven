using ReadHaven.Models;
using ReadHaven.Models.Enums;

namespace ReadHaven.ViewModels
{
    public class OrderViewModel
    {
        public decimal Amount { get; set; }
        public string ShippingAddress { get; set; }
        public string Email { get; set; } = string.Empty;
        public City ShippingCity { get; set; }
        public string ShippingPostalCode { get; set; }
        public Country ShippingCountry { get; set; }
        public string ShippingContact { get; set; }
        public Currency Currency { get; set; } = Currency.USD;  
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Email; 
    }
}
