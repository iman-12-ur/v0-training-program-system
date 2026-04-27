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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // TrainingProgram Configuration
            modelBuilder.Entity<TrainingProgram>(entity =>
            {
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.CreatedAt);
                
                entity.HasMany(e => e.Batches)
                      .WithOne(b => b.TrainingProgram)
                      .HasForeignKey(b => b.TrainingProgramId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Batch Configuration
            modelBuilder.Entity<Batch>(entity =>
            {
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.StartDate);
                
                entity.HasMany(e => e.Registrations)
                      .WithOne(r => r.Batch)
                      .HasForeignKey(r => r.BatchId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Registration Configuration
            modelBuilder.Entity<Registration>(entity =>
            {
                entity.HasIndex(e => e.EmployeeId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.RegisteredAt);
                
                // Prevent duplicate registration
                entity.HasIndex(e => new { e.EmployeeId, e.BatchId }).IsUnique();
            });

            // Seed Default Settings
            modelBuilder.Entity<SystemSettings>().HasData(
                new SystemSettings
                {
                    Id = 1,
                    WelcomeTitle = "مرحباً بك في بوابة التدريب",
                    WelcomeDescription = "استعرض البرامج التدريبية المتاحة وقدّم طلب ترشيحك للبرنامج المناسب.",
                    AdminWelcome = "مرحباً،",
                    AdminDescription = "لوحة إدارة البرامج التدريبية - يمكنك إدارة البرامج والتسجيلات من هنا",
                    OrganizationName = "المجلس الأعلى للقضاء",
                    UpdatedAt = DateTime.Now
                }
            );
        }
    }
}
