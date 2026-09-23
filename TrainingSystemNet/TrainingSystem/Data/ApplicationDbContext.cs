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
        public DbSet<TrainingNeed> TrainingNeeds { get; set; }
        public DbSet<TrainingNeedBatch> TrainingNeedBatches { get; set; }
        public DbSet<SkillCategory> SkillCategories { get; set; }
        public DbSet<Skill> Skills { get; set; }
        public DbSet<TrainingNeedStatusHistory> TrainingNeedStatusHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure TrainingProgram
            builder.Entity<TrainingProgram>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).IsRequired();
            });

            // Configure Batch - علاقة مع البرنامج التدريبي
            builder.Entity<Batch>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                
                entity.HasOne(e => e.TrainingProgram)
                      .WithMany(p => p.Batches)
                      .HasForeignKey(e => e.TrainingProgramId)
                      .OnDelete(DeleteBehavior.Restrict); // تغيير من Cascade إلى Restrict
            });

            // Configure Registration - علاقة مع الدفعة فقط
            builder.Entity<Registration>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.VisitorName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.EmployeeId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Phone).IsRequired().HasMaxLength(20);
                
                // علاقة واحدة فقط مع Batch - لا توجد علاقة مباشرة مع TrainingProgram
                entity.HasOne(e => e.Batch)
                      .WithMany(b => b.Registrations)
                      .HasForeignKey(e => e.BatchId)
                      .OnDelete(DeleteBehavior.Restrict); // استخدام Restrict بدلاً من Cascade
            });

            // Configure SystemSettings
            builder.Entity<SystemSettings>(entity =>
            {
                entity.HasKey(e => e.Id);
            });

            // Configure TrainingNeedBatch - دفعة رفع الاحتياجات
            builder.Entity<TrainingNeedBatch>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Department).IsRequired().HasMaxLength(100);
            });

            // Configure TrainingNeed - سجل الاحتياج التدريبي
            builder.Entity<TrainingNeed>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.EmployeeName).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Department).IsRequired().HasMaxLength(100);
                entity.Property(e => e.SkillName).IsRequired().HasMaxLength(150);

                entity.HasOne(e => e.Batch)
                      .WithMany(b => b.Needs)
                      .HasForeignKey(e => e.BatchId)
                      .OnDelete(DeleteBehavior.SetNull);

                // فهرس على الدائرة لتسريع التصفية حسب الدائرة
                entity.HasIndex(e => e.Department);
                entity.HasIndex(e => e.ApprovalStatus);

                // ربط اختياري بالموظف (بدون حذف متسلسل لتفادي مسارات متعددة)
                entity.HasOne(e => e.Employee)
                      .WithMany()
                      .HasForeignKey(e => e.EmployeeUserId)
                      .OnDelete(DeleteBehavior.NoAction);

                // ربط اختياري بمهارة معرّفة
                entity.HasOne(e => e.SkillRef)
                      .WithMany()
                      .HasForeignKey(e => e.SkillId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure SkillCategory - تصنيف المهارات
            builder.Entity<SkillCategory>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            });

            // Configure Skill - مهارة ضمن تصنيف
            builder.Entity<Skill>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(150);

                entity.HasOne(e => e.SkillCategory)
                      .WithMany(c => c.Skills)
                      .HasForeignKey(e => e.SkillCategoryId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure TrainingNeedStatusHistory - سجل مراحل الاعتماد
            builder.Entity<TrainingNeedStatusHistory>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.TrainingNeed)
                      .WithMany(n => n.StatusHistory)
                      .HasForeignKey(e => e.TrainingNeedId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
