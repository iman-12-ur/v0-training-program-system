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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure TrainingProgram
            modelBuilder.Entity<TrainingProgram>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).IsRequired();
                entity.Property(e => e.Duration).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Instructor).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Location).IsRequired().HasMaxLength(200);
            });

            // Configure Batch
            modelBuilder.Entity<Batch>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.HasOne(e => e.TrainingProgram)
                      .WithMany(p => p.Batches)
                      .HasForeignKey(e => e.TrainingProgramId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Registration
            modelBuilder.Entity<Registration>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.VisitorName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.EmployeeId).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Phone).IsRequired().HasMaxLength(20);
                entity.HasOne(e => e.Batch)
                      .WithMany(b => b.Registrations)
                      .HasForeignKey(e => e.BatchId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Seed default admin user
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed sample training program
            modelBuilder.Entity<TrainingProgram>().HasData(
                new TrainingProgram
                {
                    Id = 1,
                    Title = "القيادة الفعّالة",
                    Description = "برنامج تدريبي شامل لتطوير المهارات القيادية والإدارية للمشرفين والمدراء",
                    Categories = "القيادة والإدارة",
                    ProgramType = "تطوير مهني",
                    TargetAudience = "المشرفين والمدراء",
                    Duration = "5 أيام",
                    Instructor = "د. أحمد محمد",
                    Location = "قاعة التدريب الرئيسية",
                    Objectives = "فهم أساسيات القيادة الفعالة\nتطوير مهارات التواصل\nبناء فرق عمل متماسكة",
                    Topics = "مفهوم القيادة وأنماطها\nمهارات التأثير والإقناع\nإدارة فرق العمل",
                    Prerequisites = "خبرة لا تقل عن سنتين\nموافقة المدير المباشر",
                    Status = ProgramStatus.Active,
                    CreatedAt = DateTime.Now
                }
            );

            // Seed sample batch
            modelBuilder.Entity<Batch>().HasData(
                new Batch
                {
                    Id = 1,
                    Name = "الدفعة الأولى",
                    StartDate = DateTime.Now.AddDays(30),
                    EndDate = DateTime.Now.AddDays(35),
                    MaxParticipants = 25,
                    CurrentParticipants = 0,
                    Status = BatchStatus.Upcoming,
                    TrainingProgramId = 1
                }
            );
        }
    }
}
