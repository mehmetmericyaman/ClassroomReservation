using Microsoft.EntityFrameworkCore;
using Ceng382ClassroomReservation.Models;

namespace Ceng382ClassroomReservation.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Classroom> Classrooms { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Term> Terms { get; set; }

        public DbSet<Feedback> Feedbacks { get; set; }

        public DbSet<LogEntry> Logs { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Tablo adlarını manuel olarak belirtiyoruz (çünkü migration kullanmıyoruz)
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Term>().ToTable("Terms");
            modelBuilder.Entity<Classroom>().ToTable("Classrooms");
            modelBuilder.Entity<Reservation>().ToTable("Reservations");
            modelBuilder.Entity<Feedback>().ToTable("Feedbacks");


            // İlişkiler
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reservations)
                .HasForeignKey(r => r.UserId);

            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Classroom)
                .WithMany(c => c.Reservations)
                .HasForeignKey(r => r.ClassroomId);

            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Term)
                .WithMany(t => t.Reservations)
                .HasForeignKey(r => r.TermId);
        }
    }
}
