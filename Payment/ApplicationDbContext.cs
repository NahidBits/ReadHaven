using Microsoft.EntityFrameworkCore;
using Payment.Models;

namespace Payment.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        public DbSet<PaymentModel> Payments { get; set; }
        public DbSet<OtpRequest> OtpRequests { get; set; }
    }
}
