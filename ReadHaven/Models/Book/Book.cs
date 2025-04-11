using ReadHaven.Models.Common;

namespace ReadHaven.Models.Book
{
    public class Book : BaseEntity
    {
        public string Title { get; set; } 
        public string Genre { get; set; } 
        public decimal Price { get; set; }
        public string? ImagePath { get; set; }
    }   
}
