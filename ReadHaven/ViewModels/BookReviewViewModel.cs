using ReadHaven.Models.Book;
using ReadHaven.Models.Common;
using ReadHaven.Models.Enums;

namespace ReadHaven.ViewModels
{
    public class BookReviewViewModel
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string ReviewText { get; set; }
        public DateOnly Date { get; set; }
        public BookRating Rating { get; set; }
    }
}