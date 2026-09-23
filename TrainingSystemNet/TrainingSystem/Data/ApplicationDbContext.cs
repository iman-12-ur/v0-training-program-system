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
        public DbSet<TrainingBudget> TrainingBudgets { get; set; }
        public DbSet<TrainingImpactAssessment> TrainingImpactAssessments { get; set; }
        public DbSet<Notification> Notifications { get; set; }

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

                // ربط اختياري ببرنامج تدريبي معتمد
                entity.HasOne(e => e.LinkedTrainingProgram)
                      .WithMany()
                      .HasForeignKey(e => e.LinkedTrainingProgramId)
                      .OnDelete(DeleteBehavior.SetNull);

                // ربط اختياري بدفعة تدريبية تابعة للبرنامج
                entity.HasOne(e => e.TrainingBatch)
                      .WithMany()
                      .HasForeignKey(e => e.TrainingBatchId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.Property(e => e.EstimatedCost).HasColumnType("decimal(18,2)");
                entity.Property(e => e.PlannedCost).HasColumnType("decimal(18,2)");
                entity.Property(e => e.ActualCost).HasColumnType("decimal(18,2)");
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

            // Configure TrainingBudget - موازنة مركزية للسنة المالية
            builder.Entity<TrainingBudget>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FinancialYear).IsRequired().HasMaxLength(9);
                entity.Property(e => e.AllocatedBudget).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CommittedBudget).HasColumnType("decimal(18,2)");
                entity.Property(e => e.ActualSpending).HasColumnType("decimal(18,2)");

                // موازنة واحدة فقط لكل سنة مالية
                entity.HasIndex(e => e.FinancialYear).IsUnique();
            });

            // Configure TrainingImpactAssessment - تقييم الأثر
            builder.Entity<TrainingImpactAssessment>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.TrainingNeed)
                      .WithMany(n => n.ImpactAssessments)
                      .HasForeignKey(e => e.TrainingNeedId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.DueDate);
            });

            // Configure Notification - الإشعارات
            builder.Entity<Notification>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.Message).IsRequired().HasMaxLength(500);

                entity.HasIndex(e => new { e.UserId, e.IsRead });
            });
        }
    }
}
