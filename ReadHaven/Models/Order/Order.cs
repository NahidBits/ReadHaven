using ReadHaven.Models.Common;
using ReadHaven.Models.User;

namespace ReadHaven.Models.Order
{
    public class Order : BaseEntity
    {
        public Guid UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Pending";
    }
}
