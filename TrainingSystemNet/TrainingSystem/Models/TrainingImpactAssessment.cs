using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrainingSystem.Models
{
    // نوع تقييم الأثر
    public enum ImpactAssessmentType
    {
        PostTraining = 0, // مباشرة بعد التدريب
        Day90 = 1         // بعد 90 يوماً
    }

    // تقييم أثر التدريب على أداء الموظف
    public class TrainingImpactAssessment
    {
        public int Id { get; set; }

        [Required]
        public int TrainingNeedId { get; set; }
        public TrainingNeed? TrainingNeed { get; set; }

        [Display(Name = "نوع التقييم")]
        public ImpactAssessmentType AssessmentType { get; set; }

        // محاور التقييم (1-5)
        [Display(Name = "تطبيق المعرفة المكتسبة")]
        [Range(0, 5)]
        public int KnowledgeApplication { get; set; }

        [Display(Name = "تحسّن المهارة/الأداء")]
        [Range(0, 5)]
        public int SkillImprovement { get; set; }

        [Display(Name = "الأثر على العمل")]
        [Range(0, 5)]
        public int WorkImpact { get; set; }

        [Display(Name = "انخفاض الأخطاء")]
        [Range(0, 5)]
        public int ErrorReduction { get; set; }

        [Display(Name = "تحسّن الإنتاجية")]
        [Range(0, 5)]
        public int ProductivityGain { get; set; }

        [Display(Name = "القدرة على التطبيق العملي")]
        [Range(0, 5)]
        public int ApplicationAbility { get; set; }

        [Display(Name = "المستوى قبل التدريب")]
        [Range(0, 5)]
        public int BeforeScore { get; set; }

        [Display(Name = "المستوى بعد التدريب")]
        [Range(0, 5)]
        public int AfterScore { get; set; }

        [Display(Name = "ملاحظات المدير")]
        [MaxLength(1000)]
        public string? ManagerNotes { get; set; }

        [Display(Name = "تاريخ الاستحقاق")]
        public DateTime? DueDate { get; set; }

        public DateTime? CompletedAt { get; set; }

        [MaxLength(450)]
        public string? CompletedByUserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // هل اكتمل التقييم؟
        [NotMapped]
        public bool IsCompleted => CompletedAt.HasValue;

        // متوسط المحاور الستة
        [NotMapped]
        public double AverageScore =>
            Math.Round((KnowledgeApplication + SkillImprovement + WorkImpact +
                        ErrorReduction + ProductivityGain + ApplicationAbility) / 6.0, 1);

        // الفرق المطلق قبل/بعد: Improvement = AfterScore - BeforeScore
        [NotMapped]
        public int Improvement => AfterScore - BeforeScore;

        // هل يمكن حساب النسبة؟ (لا تُحسب إذا BeforeScore = 0)
        [NotMapped]
        public bool HasImprovementPercent => BeforeScore > 0;

        // نسبة التحسّن قبل/بعد = ((After - Before) ÷ Before) × 100 — تتجنب القسمة على صفر
        [NotMapped]
        public int ImprovementPercent => HasImprovementPercent
            ? (int)Math.Round(100.0 * (AfterScore - BeforeScore) / BeforeScore)
            : 0;

        public static string GetTypeDisplayName(ImpactAssessmentType t) => t switch
        {
            ImpactAssessmentType.Day90 => "تقييم بعد 90 يوماً",
            _ => "تقييم مباشر بعد التدريب"
        };
    }
}
