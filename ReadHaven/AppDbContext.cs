using Microsoft.EntityFrameworkCore;
using ReadHaven.Models.User;
using ReadHaven.Models.Book;    


namespace ReadHaven
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }      
        
        public DbSet<User> Users { get; set; }    
        public DbSet<Book> Books { get; set; }      
    }
}
