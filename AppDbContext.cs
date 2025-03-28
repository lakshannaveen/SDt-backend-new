using Microsoft.EntityFrameworkCore;

namespace sdt_backend.net.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");

                entity.Property(u => u.Name)
                    .HasMaxLength(100)  // Adjusted to a more reasonable length
                    .IsRequired()
                    .HasColumnType("VARCHAR(100)");

                entity.Property(u => u.Email)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasColumnType("VARCHAR(255)");

                entity.Property(u => u.Password)
                    .IsRequired()
                    .HasColumnType("TEXT");

                // Removed CreatedAt field since it's no longer needed

                entity.HasIndex(u => u.Email).IsUnique();
            });
        }
    }
}
