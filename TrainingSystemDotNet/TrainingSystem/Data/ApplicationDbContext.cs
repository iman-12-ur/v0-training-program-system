using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.Models;

namespace TrainingSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<TrainingProgram> TrainingPrograms { get; set; }
        public DbSet<Batch> Batches { get; set; }
        public DbSet<Registration> Registrations { get; set; }
        public DbSet<SystemSettings> SystemSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure TrainingProgram
            builder.Entity<TrainingProgram>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).IsRequired();
                entity.Property(e => e.Duration).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Instructor).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Location).IsRequired().HasMaxLength(200);
            });

            // Configure Batch
            builder.Entity<Batch>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.HasOne(e => e.TrainingProgram)
                      .WithMany(p => p.Batches)
                      .HasForeignKey(e => e.TrainingProgramId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Registration
            builder.Entity<Registration>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.VisitorName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.EmployeeId).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.HasOne(e => e.Batch)
                      .WithMany(b => b.Registrations)
                      .HasForeignKey(e => e.BatchId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
