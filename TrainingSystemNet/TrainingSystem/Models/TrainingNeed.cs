using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrainingSystem.Models
{
    // أولوية الاحتياج التدريبي (تُحسب من فجوة المهارة)
    public enum TrainingNeedPriority
    {
        Low = 0,     // منخفضة
        Medium = 1,  // متوسطة
        High = 2     // عالية
    }

    // حالة معالجة الاحتياج
    public enum TrainingNeedStatus
    {
        New = 0,         // جديد
        InProgress = 1,  // قيد المعالجة
        Completed = 2    // مكتمل
    }

    // حالة اعتماد الاحتياج (سير العمل: موظف ← مدير ← موارد بشرية)
    public enum TrainingNeedApprovalStatus
    {
        Draft = 0,              // مسودة (لم تُرسل بعد)
        SubmittedToManager = 1, // بانتظار اعتماد المدير المباشر
        ManagerApproved = 2,    // اعتمده المدير - بانتظار الموارد البشرية
        ManagerRejected = 3,    // رفضه المدير
        HRApproved = 4,         // اعتمدته الموارد البشرية (نهائي)
        HRRejected = 5          // رفضته الموارد البشرية
    }

    // سجل احتياج تدريبي لموظف في مهارة محددة
    public class TrainingNeed
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "اسم الموظف")]
        [MaxLength(150)]
        public string EmployeeName { get; set; } = string.Empty;

        [Display(Name = "الرقم الوظيفي")]
        [MaxLength(50)]
        public string? EmployeeNumber { get; set; }

        [Required]
        [Display(Name = "الدائرة")]
        [MaxLength(100)]
        public string Department { get; set; } = string.Empty;

        [Required]
        [Display(Name = "المهارة")]
        [MaxLength(150)]
        public string SkillName { get; set; } = string.Empty;

        [Display(Name = "التصنيف")]
        [MaxLength(80)]
        public string? Category { get; set; }

        [Display(Name = "المستوى المطلوب")]
        [Range(1, 5)]
        public int RequiredLevel { get; set; }

        [Display(Name = "المستوى الحالي")]
        [Range(0, 5)]
        public int CurrentLevel { get; set; }

        [Display(Name = "الأولوية")]
        public TrainingNeedPriority Priority { get; set; }

        [Display(Name = "الحالة")]
        public TrainingNeedStatus Status { get; set; } = TrainingNeedStatus.New;

        [Display(Name = "ملاحظات")]
        [MaxLength(500)]
        public string? Notes { get; set; }

        // ربط اختياري بدفعة رفع (عند الاستيراد من Excel)
        public int? BatchId { get; set; }
        public TrainingNeedBatch? Batch { get; set; }

        [MaxLength(450)]
        public string? CreatedByUserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // --- حقول وحدة TNA (كلها اختيارية للحفاظ على التوافق) ---

        // ربط اختياري بالموظف من جدول المستخدمين
        [MaxLength(450)]
        [Display(Name = "الموظف")]
        public string? EmployeeUserId { get; set; }
        public ApplicationUser? Employee { get; set; }

        // ربط اختياري بمهارة معرّفة مسبقاً
        [Display(Name = "المهارة (من القائمة)")]
        public int? SkillId { get; set; }
        public Skill? SkillRef { get; set; }

        [Display(Name = "المسمى الوظيفي")]
        [MaxLength(150)]
        public string? JobTitle { get; set; }

        [Display(Name = "الدرجة الوظيفية")]
        [MaxLength(50)]
        public string? Grade { get; set; }

        [Display(Name = "مبرر الاحتياج")]
        [MaxLength(1000)]
        public string? Justification { get; set; }

        [Display(Name = "الفترة الزمنية المقترحة")]
        [MaxLength(100)]
        public string? SuggestedTimeframe { get; set; }

        // --- سير الاعتماد ---
        [Display(Name = "حالة الاعتماد")]
        public TrainingNeedApprovalStatus ApprovalStatus { get; set; } = TrainingNeedApprovalStatus.Draft;

        [MaxLength(450)]
        public string? ManagerUserId { get; set; }
        public DateTime? ManagerActionAt { get; set; }
        [MaxLength(500)]
        public string? ManagerComment { get; set; }

        [MaxLength(450)]
        public string? HRUserId { get; set; }
        public DateTime? HRActionAt { get; set; }
        [MaxLength(500)]
        public string? HRComment { get; set; }

        public DateTime? SubmittedAt { get; set; }

        public List<TrainingNeedStatusHistory> StatusHistory { get; set; } = new();

        // الفجوة المحسوبة (لا تُخزّن في قاعدة البيانات)
        [NotMapped]
        public int Gap => Math.Max(0, RequiredLevel - CurrentLevel);

        // حساب الأولوية من الفجوة (منطق موحّد للخادم والاستيراد)
        public static TrainingNeedPriority ComputePriority(int requiredLevel, int currentLevel)
        {
            var gap = Math.Max(0, requiredLevel - currentLevel);
            if (gap >= 3) return TrainingNeedPriority.High;
            if (gap == 2) return TrainingNeedPriority.Medium;
            return TrainingNeedPriority.Low;
        }

        public static string GetPriorityDisplayName(TrainingNeedPriority p) => p switch
        {
            TrainingNeedPriority.High => "عالية",
            TrainingNeedPriority.Medium => "متوسطة",
            _ => "منخفضة"
        };

        public static string GetStatusDisplayName(TrainingNeedStatus s) => s switch
        {
            TrainingNeedStatus.InProgress => "قيد المعالجة",
            TrainingNeedStatus.Completed => "مكتمل",
            _ => "جديد"
        };

        public static string GetApprovalStatusDisplayName(TrainingNeedApprovalStatus s) => s switch
        {
            TrainingNeedApprovalStatus.Draft => "مسودة",
            TrainingNeedApprovalStatus.SubmittedToManager => "بانتظار المدير",
            TrainingNeedApprovalStatus.ManagerApproved => "بانتظار الموارد البشرية",
            TrainingNeedApprovalStatus.ManagerRejected => "مرفوض من المدير",
            TrainingNeedApprovalStatus.HRApproved => "معتمد نهائياً",
            TrainingNeedApprovalStatus.HRRejected => "مرفوض من الموارد البشرية",
            _ => "مسودة"
        };

        // لون شارة الحالة (Bootstrap)
        public static string GetApprovalStatusBadge(TrainingNeedApprovalStatus s) => s switch
        {
            TrainingNeedApprovalStatus.Draft => "secondary",
            TrainingNeedApprovalStatus.SubmittedToManager => "info",
            TrainingNeedApprovalStatus.ManagerApproved => "primary",
            TrainingNeedApprovalStatus.ManagerRejected => "danger",
            TrainingNeedApprovalStatus.HRApproved => "success",
            TrainingNeedApprovalStatus.HRRejected => "danger",
            _ => "secondary"
        };
    }

    // دفعة رفع احتياجات (تجمع سجلات ملف Excel واحد لدائرة معيّنة)
    public class TrainingNeedBatch
    {
        public int Id { get; set; }

        [Display(Name = "اسم الملف")]
        [MaxLength(200)]
        public string? FileName { get; set; }

        [Required]
        [Display(Name = "الدائرة")]
        [MaxLength(100)]
        public string Department { get; set; } = string.Empty;

        [Display(Name = "عدد السجلات")]
        public int RecordCount { get; set; }

        [MaxLength(450)]
        public string? CreatedByUserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public List<TrainingNeed> Needs { get; set; } = new();
    }

    // صف استيراد احتياج من ملف Excel (للمعاينة قبل الحفظ)
    public class TrainingNeedImportRow
    {
        public int RowNumber { get; set; }

        public string? EmployeeNumber { get; set; }
        public string? EmployeeName { get; set; }
        public string? Department { get; set; }
        public string? SkillName { get; set; }
        public string? Category { get; set; }
        public int RequiredLevel { get; set; }
        public int CurrentLevel { get; set; }
        public string? Notes { get; set; }

        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}
