using FastFood.Models;
using Microsoft.EntityFrameworkCore;

namespace FastFood.Data
{
	public class FastFoodDbContext : DbContext
	{
		public FastFoodDbContext()
		{
		}

		public FastFoodDbContext(DbContextOptions options)
			: base(options)
		{
		}

        public virtual DbSet<Order> Orders { get; set; }

        public virtual DbSet<Employee> Employees { get; set; }

        public virtual DbSet<OrderItem> OrderItems { get; set; }

        public virtual DbSet<Position> Positions { get; set; }

        public virtual DbSet<Item> Items { get; set; }

        public virtual DbSet<Category> Categories { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder builder)
		{
			if (!builder.IsConfigured)
			{
				builder.UseSqlServer(Configuration.ConnectionString);
			}
		}

		protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<OrderItem>()
                .HasKey(k => new {k.ItemId, k.OrderId});

            builder.Entity<Position>()
                .HasIndex(p => p.Name)
                .IsUnique(true);

            builder.Entity<Item>()
                .HasIndex(i => i.Name)
                .IsUnique(true);
        }
	}
}