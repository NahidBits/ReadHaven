using Microsoft.EntityFrameworkCore;
using ReadHaven.Models.User;    


namespace ReadHaven
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }      
        
        public DbSet<User> Users { get; set; }    
    }
}
