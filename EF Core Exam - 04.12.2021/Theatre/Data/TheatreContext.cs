using Theatre.Data.Models;

namespace Theatre.Data
{
    using Microsoft.EntityFrameworkCore;

    public class TheatreContext : DbContext
    {
        public TheatreContext() { }

        public TheatreContext(DbContextOptions options)
            : base(options) { }

        public virtual DbSet<Ticket> Tickets { get; set; }

        public virtual DbSet<Play> Plays { get; set; }

        public virtual DbSet<Models.Theatre> Theatres { get; set; }

        public virtual DbSet<Cast> Casts { get; set; }

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
