using ReadHaven.Models;
using ReadHaven.Models.Enums;

namespace ReadHaven.ViewModels
{
    public class OrderViewModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string ShippingAddress { get; set; }
        public City ShippingCity { get; set; }
        public string ShippingPostalCode { get; set; }
        public Country ShippingCountry { get; set; }
        public string ShippingContact { get; set; }
    }
}
