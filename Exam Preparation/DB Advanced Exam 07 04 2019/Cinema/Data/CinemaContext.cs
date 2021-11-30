using Cinema.Data.Models;

namespace Cinema.Data
{
    using Microsoft.EntityFrameworkCore;

    public class CinemaContext : DbContext
    {
        public CinemaContext()  { }

        public CinemaContext(DbContextOptions options)
            : base(options)   { }

        public virtual DbSet<Seat> Seats { get; set; }

        public virtual DbSet<Ticket> Tickets { get; set; }

        public virtual DbSet<Customer> Customers { get; set; }

        public virtual DbSet<Hall> Halls { get; set; }

        public virtual DbSet<Projection> Projections { get; set; }

        public virtual DbSet<Movie> Movies { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder
                    .UseSqlServer(Configuration.ConnectionString);
            }
        }
    }
}