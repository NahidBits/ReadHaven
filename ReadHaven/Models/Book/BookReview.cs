using ReadHaven.Models.Common;
using ReadHaven.Models.Enums;

namespace ReadHaven.Models.Book
{
    public class BookReview : BaseEntity
    {
        public string ReviewText { get; set; }
        public BookRating Rating { get; set; }
        public Guid UserId { get; set; }
        public Guid BookId { get; set; }
    }
}