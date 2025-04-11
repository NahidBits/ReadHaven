using Microsoft.EntityFrameworkCore;
using ReadHaven.Models.User;
using ReadHaven.Models.Book;
using ReadHaven.Models.Common;
using ReadHaven.Models.Cart;


namespace ReadHaven
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }      
        
        public DbSet<User> Users { get; set; }    
        public DbSet<Book> Books { get; set; }
        public DbSet<BookReview> BookReviews { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<ResetPasswordToken> ResetPasswordToken { get; set; }

        public DbSet<CartItem> CartItems { get; set; } 

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply global filter to all entities to exclude soft-deleted records
            modelBuilder.Entity<Book>().HasQueryFilter(b => !b.IsDeleted);
            modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
            modelBuilder.Entity<BookReview>().HasQueryFilter(u => !u.IsDeleted);
            modelBuilder.Entity<UserRole>().HasQueryFilter(u => !u.IsDeleted);
            modelBuilder.Entity<ResetPasswordToken>().HasQueryFilter(u => !u.IsDeleted);
            modelBuilder.Entity<CartItem>().HasQueryFilter(u => !u.IsDeleted);
        }

        public override int SaveChanges()
        {
            SetUpdatedAtForModifiedEntities();
            HandleSoftDeletes();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SetUpdatedAtForModifiedEntities();
            HandleSoftDeletes();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void SetUpdatedAtForModifiedEntities()
        {
            var modifiedEntities = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Modified)
                .ToList();

            foreach (var entity in modifiedEntities)
            {
                if (entity.Entity is BaseEntity baseEntity)
                {
                    baseEntity.UpdatedAt = DateTime.Now;
                }
            }
        }

        private void HandleSoftDeletes()
        {
            var deletedEntities = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Deleted)
                .ToList();

            foreach (var entity in deletedEntities)
            {
                if (entity.Entity is BaseEntity baseEntity)
                {
                    // Mark entity as deleted instead of actually deleting it
                    baseEntity.IsDeleted = true;
                    entity.State = EntityState.Modified;  
                }
            }
        }
    }
}
