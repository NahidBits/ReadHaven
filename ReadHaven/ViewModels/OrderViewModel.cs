using ReadHaven.Models;
using ReadHaven.Models.Enums;

namespace ReadHaven.ViewModels
{
    public class OrderViewModel
    {
        public string ShippingAddress { get; set; }
        public string Email { get; set; } = string.Empty;
        public City ShippingCity { get; set; }
        public string ShippingPostalCode { get; set; }
        public Country ShippingCountry { get; set; }
        public string ShippingContact { get; set; }
    }
}
