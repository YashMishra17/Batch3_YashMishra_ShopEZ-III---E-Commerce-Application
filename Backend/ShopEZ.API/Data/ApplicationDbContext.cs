using Microsoft.EntityFrameworkCore;
using ShopEZ.API.Models;

namespace ShopEZ.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<OrderItem> OrderItems { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── Relationships ──────────────────────────────────────────────────

            // One User → Many Orders
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // One Order → Many OrderItems  (cascade delete)
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // One Product → Many OrderItems  (restrict delete)
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── Column Precision ───────────────────────────────────────────────

            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.Price)
                .HasColumnType("decimal(18,2)");

            // ── Seed Data ──────────────────────────────────────────────────────

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    UserId = 1,
                    Name = "Alice Johnson",
                    Email = "alice@shopez.com",
                    Password = "hashed_password_alice",
                    Role = "Admin"
                },
                new User
                {
                    UserId = 2,
                    Name = "Bob Smith",
                    Email = "bob@shopez.com",
                    Password = "hashed_password_bob",
                    Role = "Customer"
                }
            );

            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    ProductId = 1,
                    Name = "Wireless Mouse",
                    Description = "Ergonomic wireless mouse with USB receiver",
                    Price = 29.99m,
                    ImageUrl = "https://via.placeholder.com/300?text=Wireless+Mouse",
                    Stock = 100
                },
                new Product
                {
                    ProductId = 2,
                    Name = "Mechanical Keyboard",
                    Description = "RGB mechanical keyboard with blue switches",
                    Price = 79.99m,
                    ImageUrl = "https://via.placeholder.com/300?text=Mechanical+Keyboard",
                    Stock = 50
                },
                new Product
                {
                    ProductId = 3,
                    Name = "USB-C Hub",
                    Description = "7-in-1 USB-C hub with HDMI and SD card reader",
                    Price = 49.99m,
                    ImageUrl = "https://via.placeholder.com/300?text=USB-C+Hub",
                    Stock = 75
                }
            );
        }
    }
}
