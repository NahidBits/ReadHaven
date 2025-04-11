using ReadHaven.Models.Common;

namespace ReadHaven.Models.Cart
{
    public class CartItem : BaseEntity
    {
        public Guid UserId { get; set; }
        public Guid BookId { get; set; }
        public int Quantity { get; set; }  
        public decimal Price { get; set; } 
    }
}
