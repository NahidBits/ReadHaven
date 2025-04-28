using ReadHaven.Models.Common;
using ReadHaven.Models.Enums;
using ReadHaven.Models.User;

namespace ReadHaven.Models.Order
{
    public class Order : BaseEntity
    {
        public Guid UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public string ShippingAddress { get; set; }    
        public string Email { get; set; } = string.Empty;   
        public City ShippingCity { get; set; }          
        public string ShippingPostalCode { get; set; }     
        public Country ShippingCountry { get; set; }        
        public string ShippingContact { get; set; }
        public DateTime PossibleDayToShip { get; set; }
    }
}
