using ReadHaven.Models.Common;

namespace ReadHaven.Models.Book
{
    public class Wishlist : BaseEntity
    {
       public Guid UserId { get; set; }
       public Guid BookId { get; set; }   
       public bool IsLoved { get; set; } = false; 
    }
}
