using Microsoft.EntityFrameworkCore;
using P01_StudentSystem.Data.Models;

namespace P01_StudentSystem.Data
{
    public class StudentSystemContext : DbContext
    {
        public StudentSystemContext()
        {
        }

        public StudentSystemContext(DbContextOptions<StudentSystemContext> opt)
            : base(opt)
        {
        }

        public virtual DbSet<Student> Students { get; set; }
        public virtual  DbSet<Course> Courses { get; set; }
        public virtual DbSet<Homework> HomeworkSubmissions { get; set; }
        public virtual DbSet<Resource> Resources { get; set; }
        public virtual  DbSet<StudentCourse> StudentCourses { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=.;Database=StudentSystem;Integrated Security=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Student>()
                .Property(x => x.PhoneNumber)
                .HasColumnType("varchar(10)")
                .IsUnicode(false);

            modelBuilder.Entity<Resource>()
                .Property(x => x.Url)
                .HasColumnType("varchar(2048)")
                .IsUnicode(false);

            modelBuilder.Entity<Homework>()
                .Property(x => x.Content)
                .HasColumnType("varchar(255)")
                .IsUnicode(false);

            modelBuilder.Entity<StudentCourse>()
                .HasKey(x => new {x.CourseId, x.StudentId});

            modelBuilder.Entity<StudentCourse>()
                .HasOne(sc => sc.Student)
                .WithMany(s => s.CourseEnrollments)
                .HasForeignKey(sc => sc.StudentId);

            modelBuilder.Entity<StudentCourse>()
                .HasOne(sc => sc.Course)
                .WithMany(c => c.StudentsEnrolled)
                .HasForeignKey(sc => sc.CourseId);
        }
    }
}
