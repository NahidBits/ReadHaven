using ReadHaven.Models.Book;
using ReadHaven.Models.Common;
using ReadHaven.Models.Enums;

namespace ReadHaven.ViewModels
{
    public class BookDetailsViewModel 
    {
        public Book Book { get; set; }
        public BookRating Rating { get; set; }  
        
        public BookReview? UserReview { get; set; }
        public List<BookReview> Reviews { get; set; } = new List<BookReview>();
    }
}
