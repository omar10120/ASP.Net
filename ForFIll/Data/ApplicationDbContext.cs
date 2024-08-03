



using Microsoft.EntityFrameworkCore;

using ForFIll.Models;

namespace ForFIll.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }

        // DbSet for Bills
        public DbSet<User> User { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Specify the column type for the Price property
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<User>()
            .Property(p => p.Username)
            .HasColumnType("nvarchar(MAX)");


        }
    }
  
}
