using PetClinic.Models;

namespace PetClinic.Data
{
    using Microsoft.EntityFrameworkCore;

    public class PetClinicContext : DbContext
    {
        public PetClinicContext()
        {
        }

        public PetClinicContext(DbContextOptions options)
            : base(options)
        {
        }

        public virtual DbSet<Procedure> Procedures { get; set; }

        public virtual DbSet<ProcedureAnimalAid> ProceduresAnimalAids { get; set; }

        public virtual DbSet<Animal> Animals { get; set; }

        public virtual DbSet<AnimalAid> AnimalAids { get; set; }

        public virtual DbSet<Passport> Passports { get; set; }

        public virtual DbSet<Vet> Vets { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(Configuration.ConnectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Vet>()
                .HasIndex(i => i.PhoneNumber)
                .IsUnique();

            builder.Entity<AnimalAid>()
                .HasIndex(i => i.Name)
                .IsUnique();

            builder.Entity<ProcedureAnimalAid>()
                .HasKey(k => new {k.ProcedureId, k.AnimalAidId});
        }
    }
}