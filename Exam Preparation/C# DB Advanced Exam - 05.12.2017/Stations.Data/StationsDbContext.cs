using Microsoft.EntityFrameworkCore;
using Stations.Models;


namespace Stations.Data
{
	public class StationsDbContext : DbContext
	{
		public StationsDbContext()
		{
		}

		public StationsDbContext(DbContextOptions options)
			: base(options)
		{
		}

        public virtual DbSet<Station> Stations { get; set; }

        public virtual DbSet<Train> Trains { get; set; }

        public virtual DbSet<SeatingClass> SeatingClasses { get; set; }

        public virtual DbSet<CustomerCard> CustomerCards { get; set; }

        public virtual DbSet<Ticket> Tickets { get; set; }

        public virtual DbSet<TrainSeat> TrainSeats { get; set; }

        public virtual DbSet<Trip> Trips { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			if (!optionsBuilder.IsConfigured)
			{
				optionsBuilder.UseSqlServer(Configuration.ConnectionString);
			}
		}

		protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Station>()
                .HasIndex(i => i.Name)
                .IsUnique();

            builder.Entity<Station>()
                .HasMany(e => e.TripsFrom)
                .WithOne(e => e.OriginStation)
                .HasForeignKey(e => e.OriginStationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Station>()
                .HasMany(e => e.TripsTo)
                .WithOne(e => e.DestinationStation)
                .HasForeignKey(e => e.DestinationStationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Train>()
                .HasIndex(i => i.TrainNumber)
                .IsUnique();

            builder.Entity<Train>()
                .HasMany(e => e.Trips)
                .WithOne(e => e.Train)
                .HasForeignKey(e => e.TrainId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SeatingClass>()
                .HasAlternateKey(e => e.Name);

            builder.Entity<SeatingClass>()
                .HasAlternateKey(e => e.Abbreviation);

            builder.Entity<CustomerCard>()
                .HasMany(e => e.BoughtTickets)
                .WithOne(e => e.CustomerCard)
                .HasForeignKey(e => e.CustomerCardId)
                .OnDelete(DeleteBehavior.SetNull);
        }
	}
}