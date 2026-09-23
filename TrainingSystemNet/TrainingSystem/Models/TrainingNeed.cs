using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrainingSystem.Models
{
    // أولوية الاحتياج التدريبي (تُحسب من فجوة المهارة)
    public enum TrainingNeedPriority
    {
        Low = 0,       // منخفضة
        Medium = 1,    // متوسطة
        High = 2,      // عالية
        Critical = 3   // حرجة جداً
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
        SubmittedToManager = 1, // قيد مراجعة المدير المباشر
        ManagerApproved = 2,    // اعتمده المدير - بانتظار دائرة التدريب
        ManagerRejected = 3,    // رفضه المدير
        HRApproved = 4,         // اعتمدته دائرة التدريب (نهائي)
        HRRejected = 5,         // رفضته دائرة التدريب
        BudgetReview = 6,       // مراجعة الميزانية (تجاوز المتاح)
        TrainingScheduled = 7,  // مجدول للتدريب
        TrainingCompleted = 8,  // اكتمل التدريب
        // حالات مضافة (أرقام جديدة — لا تُعاد ترقمة القيم الموجودة حفاظاً على البيانات)
        ReturnedForModification = 9,   // أُعيد للتعديل من المدير
        ImpactAssessmentPending = 10,  // بانتظار تقييم أثر التدريب
        ImpactAssessmentCompleted = 11,// اكتمل تقييم الأثر
        Closed = 12                    // مغلق (انتهت دورة الطلب)
    }

    // مبرر طلب التدريب (قائمة ثابتة على مستوى المنظومة)
    public enum TrainingReason
    {
        [Display(Name = "ضعف في تقييم الأداء السنوي")]
        PerformanceWeakness = 1,

        [Display(Name = "تعثر في تحقيق هدف وظيفي")]
        GoalFailure = 2,

        [Display(Name = "استحداث نظام أو تقنية جديدة")]
        NewSystem = 3,

        [Display(Name = "تغيير تشريعي أو تنظيمي")]
        RegulatoryChange = 4,

        [Display(Name = "ترقية مستهدفة")]
        TargetedPromotion = 5,

        [Display(Name = "إعداد الصف الثاني")]
        SecondLinePreparation = 6,

        [Display(Name = "متطلبات وظيفة جديدة")]
        NewJobRequirements = 7,

        [Display(Name = "فجوة مهارية مكتشفة")]
        DiscoveredSkillGap = 8,

        [Display(Name = "متطلب إلزامي")]
        MandatoryRequirement = 9,

        [Display(Name = "تطوير مستقبلي")]
        FutureDevelopment = 10,

        [Display(Name = "أخرى")]
        Other = 99
    }

    public static class TrainingReasonExtensions
    {
        public static string DisplayName(this TrainingReason reason) => reason switch
        {
            TrainingReason.PerformanceWeakness => "ضعف في تقييم الأداء السنوي",
            TrainingReason.GoalFailure => "تعثر في تحقيق هدف وظيفي",
            TrainingReason.NewSystem => "استحداث نظام أو تقنية جديدة",
            TrainingReason.RegulatoryChange => "تغيير تشريعي أو تنظيمي",
            TrainingReason.TargetedPromotion => "ترقية مستهدفة",
            TrainingReason.SecondLinePreparation => "إعداد الصف الثاني",
            TrainingReason.NewJobRequirements => "متطلبات وظيفة جديدة",
            TrainingReason.DiscoveredSkillGap => "فجوة مهارية مكتشفة",
            TrainingReason.MandatoryRequirement => "متطلب إلزامي",
            TrainingReason.FutureDevelopment => "تطوير مستقبلي",
            TrainingReason.Other => "أخرى",
            _ => reason.ToString()
        };
    }

    // نمط تنفيذ التدريب
    public enum TrainingMode
    {
        [Display(Name = "حضوري")]
        InPerson = 1,

        [Display(Name = "عن بعد")]
        Remote = 2,

        [Display(Name = "مدمج")]
        Blended = 3
    }

    public static class TrainingModeExtensions
    {
        public static string DisplayName(this TrainingMode mode) => mode switch
        {
            TrainingMode.InPerson => "حضوري",
            TrainingMode.Remote => "عن بعد",
            TrainingMode.Blended => "مدمج",
            _ => mode.ToString()
        };
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

        // ربط اختياري بتصنيف المهارة (من جدول التصنيفات)
        [Display(Name = "تصنيف المهارة")]
        public int? SkillCategoryId { get; set; }
        public SkillCategory? SkillCategoryRef { get; set; }

        [Display(Name = "المسمى الوظيفي")]
        [MaxLength(150)]
        public string? JobTitle { get; set; }

        [Display(Name = "الدرجة الوظيفية")]
        [MaxLength(50)]
        public string? Grade { get; set; }

        // مبرر طلب التدريب (إلزامي — من القائمة الثابتة)
        [Required(ErrorMessage = "يجب اختيار مبرر طلب التدريب")]
        [Display(Name = "مبرر طلب التدريب")]
        public TrainingReason? TrainingReason { get; set; }

        // حقول شرطية تظهر عند اختيار "ضعف في تقييم الأداء السنوي"
        [Display(Name = "رقم ��لهد�� المتعثر")]
        [MaxLength(100)]
        public string? FailedGoalNumber { get; set; }

        [Display(Name = "وصف الفجوة في الأداء")]
        [MaxLength(1000)]
        public string? PerformanceGapDescription { get; set; }

        [Display(Name = "مبرر إضافي / تفاصيل المبرر")]
        [MaxLength(1000)]
        public string? Justification { get; set; }

        [Display(Name = "وصف الاحتياج")]
        [MaxLength(1000)]
        public string? NeedDescription { get; set; }

        // نوع الفجوة المهارية (أحد الأنواع الكبرى الثابتة)
        [Display(Name = "نوع الفجوة المهارية")]
        public SkillGapType? GapType { get; set; }

        // درجة الفجوة المخزّنة (تُحسب من الفرق بين المطلوب والحالي)
        [Display(Name = "درجة الفجوة")]
        public int GapScore { get; set; }

        // البرنامج التدريبي المقترح كنص (قبل ربطه ببرنامج قائم)
        [Display(Name = "البرنامج التدريبي المقترح")]
        [MaxLength(200)]
        public string? ProposedTrainingProgram { get; set; }

        // وصف مختصر للبرنامج المقترح
        [Display(Name = "وصف مختصر للبرنامج")]
        [MaxLength(500)]
        public string? ProposedProgramDescription { get; set; }

        // مزوّد التدريب المفضّل
        [Display(Name = "مزوّد التدريب المفضّل")]
        [MaxLength(200)]
        public string? PreferredTrainingProvider { get; set; }

        // نمط التدريب المقترح (حضوري / عن بعد / مدمج)
        [Display(Name = "نمط التدريب")]
        public TrainingMode? TrainingMode { get; set; }

        // المدة المقترحة للبرنامج (نصياً: مثال "5 أيام")
        [Display(Name = "المدة المقترحة")]
        [MaxLength(100)]
        public string? ProposedDuration { get; set; }

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

        // تاريخ الاعتماد النهائي (يُضبط عند اعتماد دائرة التدريب)
        [Display(Name = "تاريخ الاعتماد")]
        public DateTime? ApprovalDate { get; set; }

        // --- المرحلة 2: التكلفة والأثر والميزانية ---

        // ربط الاحتياج ببرنامج تدريبي موجود مسبقاً (لا يُنشأ برنامج جديد)
        [Display(Name = "البرنامج التدريبي")]
        public int? LinkedTrainingProgramId { get; set; }
        public TrainingProgram? LinkedTrainingProgram { get; set; }

        // ربط اختياري بدفعة تدريبية تابعة للبرنامج (Training Program → Batch → Employee)
        [Display(Name = "الدفعة التدريبية")]
        public int? TrainingBatchId { get; set; }
        public Batch? TrainingBatch { get; set; }

        [Display(Name = "التكلفة المخطّطة")]
        [Range(0, double.MaxValue)]
        public decimal PlannedCost { get; set; }

        [Display(Name = "التكلفة الفعلية")]
        [Range(0, double.MaxValue)]
        public decimal ActualCost { get; set; }

        [Display(Name = "التاريخ المخطّط")]
        [DataType(DataType.Date)]
        public DateTime? PlannedDate { get; set; }

        [Display(Name = "تاريخ الإنجاز")]
        [DataType(DataType.Date)]
        public DateTime? CompletionDate { get; set; }

        [Display(Name = "التكلفة التقديرية")]
        [Range(0, double.MaxValue)]
        public decimal EstimatedCost { get; set; }

        [Display(Name = "عدد المشاركين")]
        [Range(1, int.MaxValue)]
        public int ParticipantsCount { get; set; } = 1;

        // درجة الأثر على العمل (1-4) — تدخل في حساب الأولوية
        [Display(Name = "الأثر على العمل")]
        [Range(1, 4)]
        public int ImpactScore { get; set; } = 2;

        // درجة المخاطر عند عدم التدريب (1-4)
        [Display(Name = "درجة المخاطر")]
        [Range(1, 4)]
        public int RiskScore { get; set; } = 2;

        // احتياج امتثال/إلزامي؟
        [Display(Name = "احتياج امتثال إلزامي")]
        public bool IsCompliance { get; set; }

        // درجة الأولوية المحسوبة (0-100) — تُخزَّن للفرز والتقارير
        [Display(Name = "درجة الأولوية")]
        public int PriorityScore { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public List<TrainingNeedStatusHistory> StatusHistory { get; set; } = new();
        public List<TrainingImpactAssessment> ImpactAssessments { get; set; } = new();

        // الفجوة المحسوبة (لا تُخزّن في قاعدة البيانات)
        [NotMapped]
        public int Gap => Math.Max(0, RequiredLevel - CurrentLevel);

        // تسمية مستوى المهارة (1 مبتدئ .. 5 خبير)
        public static string LevelName(int level) => level switch
        {
            0 => "لا يوجد",
            1 => "مبتدئ",
            2 => "أساسي",
            3 => "متوسط",
            4 => "متقدم",
            5 => "خبير",
            _ => level.ToString()
        };

        // تسمية مقياس الأثر/المخاطر (1 منخفض .. 4 حرج)
        public static string ImpactRiskName(int level) => level switch
        {
            1 => "منخفض",
            2 => "متوسط",
            3 => "مرتفع",
            4 => "حرج",
            _ => level.ToString()
        };

        // حساب الأولوية من الفجوة (منطق موحّد للخادم والاستيراد)
        public static TrainingNeedPriority ComputePriority(int requiredLevel, int currentLevel)
        {
            var gap = Math.Max(0, requiredLevel - currentLevel);
            if (gap >= 3) return TrainingNeedPriority.High;
            if (gap == 2) return TrainingNeedPriority.Medium;
            return TrainingNeedPriority.Low;
        }

        // درجة الأولوية الموزونة (0-100) وفق الصيغة:
        // PriorityScore = (Gap × wGap) + (Impact × wImpact) + (Risk × wRisk) + (Compliance × wCompliance)
        // العوامل مُطبّعة إلى نسبة (0-1) ثم مضروبة بوزنها، والأوزان مأخوذة من PriorityScoringSettings
        // ليسهل تعديلها من إعدادات النظام مستقبلاً.
        public static int ComputePriorityScore(int requiredLevel, int currentLevel,
            int impact, int risk, bool isCompliance)
        {
            var gap = Math.Max(0, requiredLevel - currentLevel);
            var gapRatio = Math.Min(gap, PriorityScoringSettings.MaxGap) / (double)PriorityScoringSettings.MaxGap;
            var impactRatio = (Math.Clamp(impact, 1, PriorityScoringSettings.MaxImpact) - 1) / (double)(PriorityScoringSettings.MaxImpact - 1);
            var riskRatio = (Math.Clamp(risk, 1, PriorityScoringSettings.MaxRisk) - 1) / (double)(PriorityScoringSettings.MaxRisk - 1);
            var complianceRatio = isCompliance ? 1.0 : 0.0;

            var score = gapRatio * PriorityScoringSettings.GapWeight
                      + impactRatio * PriorityScoringSettings.ImpactWeight
                      + riskRatio * PriorityScoringSettings.RiskWeight
                      + complianceRatio * PriorityScoringSettings.ComplianceWeight;

            return Math.Clamp((int)Math.Round(score), 0, 100);
        }

        // تصنيف الأولوية من الدرجة الموزونة وفق حدود قابلة للتعديل
        public static TrainingNeedPriority ClassifyByScore(int score)
        {
            if (score >= PriorityScoringSettings.CriticalThreshold) return TrainingNeedPriority.Critical;
            if (score >= PriorityScoringSettings.HighThreshold) return TrainingNeedPriority.High;
            if (score >= PriorityScoringSettings.MediumThreshold) return TrainingNeedPriority.Medium;
            return TrainingNeedPriority.Low;
        }

        public static string GetPriorityDisplayName(TrainingNeedPriority p) => p switch
        {
            TrainingNeedPriority.Critical => "حرجة جداً",
            TrainingNeedPriority.High => "عالية",
            TrainingNeedPriority.Medium => "متوسطة",
            _ => "من��فضة"
        };

        public static string GetPriorityBadge(TrainingNeedPriority p) => p switch
        {
            TrainingNeedPriority.Critical => "dark",
            TrainingNeedPriority.High => "danger",
            TrainingNeedPriority.Medium => "warning",
            _ => "success"
        };

        public static string GetStatusDisplayName(TrainingNeedStatus s) => s switch
        {
            TrainingNeedStatus.InProgress => "��يد المعالجة",
            TrainingNeedStatus.Completed => "مكتمل",
            _ => "جديد"
        };

        public static string GetApprovalStatusDisplayName(TrainingNeedApprovalStatus s) => s switch
        {
            TrainingNeedApprovalStatus.Draft => "مسودة",
            TrainingNeedApprovalStatus.SubmittedToManager => "قيد مراجعة المدير",
            TrainingNeedApprovalStatus.ManagerApproved => "معتمد من المدير – بانتظار دائرة التدريب",
            TrainingNeedApprovalStatus.ManagerRejected => "مرفوض من المدير",
            TrainingNeedApprovalStatus.ReturnedForModification => "أُعيد للتعديل",
            TrainingNeedApprovalStatus.BudgetReview => "مراجعة الميزانية",
            TrainingNeedApprovalStatus.HRApproved => "معتمد نهائياً",
            TrainingNeedApprovalStatus.HRRejected => "مرفوض من دائرة التدريب",
            TrainingNeedApprovalStatus.TrainingScheduled => "مجدول للتدريب",
            TrainingNeedApprovalStatus.TrainingCompleted => "اكتمل التدريب",
            TrainingNeedApprovalStatus.ImpactAssessmentPending => "بانتظار تقييم الأثر",
            TrainingNeedApprovalStatus.ImpactAssessmentCompleted => "اكتمل تقييم الأثر",
            TrainingNeedApprovalStatus.Closed => "مغلق",
            _ => "مسودة"
        };

        // أيقونة الحالة (Bootstrap Icons) لعرض شارة واضحة
        public static string GetApprovalStatusIcon(TrainingNeedApprovalStatus s) => s switch
        {
            TrainingNeedApprovalStatus.Draft => "bi-pencil",
            TrainingNeedApprovalStatus.SubmittedToManager => "bi-hourglass-split",
            TrainingNeedApprovalStatus.ManagerApproved => "bi-person-check",
            TrainingNeedApprovalStatus.ManagerRejected => "bi-x-circle",
            TrainingNeedApprovalStatus.ReturnedForModification => "bi-arrow-counterclockwise",
            TrainingNeedApprovalStatus.BudgetReview => "bi-cash-coin",
            TrainingNeedApprovalStatus.HRApproved => "bi-patch-check",
            TrainingNeedApprovalStatus.HRRejected => "bi-x-octagon",
            TrainingNeedApprovalStatus.TrainingScheduled => "bi-calendar-check",
            TrainingNeedApprovalStatus.TrainingCompleted => "bi-mortarboard",
            TrainingNeedApprovalStatus.ImpactAssessmentPending => "bi-clipboard-data",
            TrainingNeedApprovalStatus.ImpactAssessmentCompleted => "bi-clipboard-check",
            TrainingNeedApprovalStatus.Closed => "bi-lock",
            _ => "bi-circle"
        };

        // لون شارة الحالة (Bootstrap)
        public static string GetApprovalStatusBadge(TrainingNeedApprovalStatus s) => s switch
        {
            TrainingNeedApprovalStatus.Draft => "secondary",
            TrainingNeedApprovalStatus.SubmittedToManager => "info",
            TrainingNeedApprovalStatus.ManagerApproved => "primary",
            TrainingNeedApprovalStatus.ManagerRejected => "danger",
            TrainingNeedApprovalStatus.ReturnedForModification => "warning",
            TrainingNeedApprovalStatus.BudgetReview => "warning",
            TrainingNeedApprovalStatus.HRApproved => "success",
            TrainingNeedApprovalStatus.HRRejected => "danger",
            TrainingNeedApprovalStatus.TrainingScheduled => "primary",
            TrainingNeedApprovalStatus.TrainingCompleted => "success",
            TrainingNeedApprovalStatus.ImpactAssessmentPending => "info",
            TrainingNeedApprovalStatus.ImpactAssessmentCompleted => "success",
            TrainingNeedApprovalStatus.Closed => "dark",
            _ => "secondary"
        };

        // --- المرحلة 10: دورة الاعتماد الموحّدة (11 مرحلة) ---
        // تُشتق حالة كل مرحلة من ApprovalStatus والحقول الموجودة (بدون أعمدة جديدة).
        public List<WorkflowStage> GetWorkflowStages()
        {
            var meta = new (string Title, string Actor)[]
            {
                ("إنشاء طلب الاحتياج", "الموظف / المدير"),
                ("التحقق من اكتمال البيانات", "النظام"),
                ("حساب الفجوة والأولوية تلقائياً", "النظام"),
                ("مراجعة واعتماد / رفض / طلب تعديل", "المدير المباشر"),
                ("تحليل الطلب", "دائرة التدريب"),
                ("ربط الاحتياج ببرنامج تدريبي", "دائرة التدريب"),
                ("التحقق من الميزانية", "دائرة التدريب"),
                ("الاعتماد النهائي", "دائرة التدريب"),
                ("إدراج الموظف في البرنامج التدريبي", "النظام"),
                ("تقييم أثر التدريب", "الموظف / المدير"),
                ("تقييم الأثر بعد 90 يوماً", "النظام (تلقائي)"),
            };

            int current;
            bool rejectedAtManager = false, rejectedAtHR = false;
            switch (ApprovalStatus)
            {
                case TrainingNeedApprovalStatus.Draft: current = 2; break;
                case TrainingNeedApprovalStatus.ReturnedForModification: current = 4; break;
                case TrainingNeedApprovalStatus.SubmittedToManager: current = 4; break;
                case TrainingNeedApprovalStatus.ManagerApproved: current = 5; break;
                case TrainingNeedApprovalStatus.ManagerRejected: current = 4; rejectedAtManager = true; break;
                case TrainingNeedApprovalStatus.BudgetReview: current = 7; break;
                case TrainingNeedApprovalStatus.HRApproved: current = 9; break;
                case TrainingNeedApprovalStatus.HRRejected: current = 8; rejectedAtHR = true; break;
                case TrainingNeedApprovalStatus.TrainingScheduled: current = 10; break;
                case TrainingNeedApprovalStatus.TrainingCompleted: current = 11; break;
                case TrainingNeedApprovalStatus.ImpactAssessmentPending: current = 11; break;
                case TrainingNeedApprovalStatus.ImpactAssessmentCompleted: current = 12; break;
                case TrainingNeedApprovalStatus.Closed: current = 12; break;
                default: current = 1; break;
            }

            bool impactDone = ImpactAssessments != null && ImpactAssessments.Any(a => a.IsCompleted);

            var list = new List<WorkflowStage>();
            for (int i = 1; i <= 11; i++)
            {
                WorkflowStageState state;
                if ((rejectedAtManager && i == 4) || (rejectedAtHR && i == 8))
                    state = WorkflowStageState.Rejected;
                else if (i < current)
                    state = WorkflowStageState.Completed;
                else if (i == current && !rejectedAtManager && !rejectedAtHR)
                    state = WorkflowStageState.Current;
                else
                    state = WorkflowStageState.Pending;

                // ترقيات مبنية على البيانات الفعلية
                if (i == 6 && LinkedTrainingProgramId != null && state == WorkflowStageState.Pending)
                    state = WorkflowStageState.Completed;
                if (i == 10 && impactDone)
                    state = WorkflowStageState.Completed;

                list.Add(new WorkflowStage
                {
                    Number = i,
                    Title = meta[i - 1].Title,
                    Actor = meta[i - 1].Actor,
                    State = state
                });
            }
            return list;
        }
    }

    // حالة مرحلة في دورة الاعتماد
    public enum WorkflowStageState { Pending, Current, Completed, Rejected }

    // مرحلة واحدة ضمن دورة الاعتماد الموحّدة
    public class WorkflowStage
    {
        public int Number { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Actor { get; set; } = string.Empty;
        public WorkflowStageState State { get; set; }

        public string BadgeClass => State switch
        {
            WorkflowStageState.Completed => "success",
            WorkflowStageState.Current => "primary",
            WorkflowStageState.Rejected => "danger",
            _ => "secondary"
        };

        public string Icon => State switch
        {
            WorkflowStageState.Completed => "bi-check-lg",
            WorkflowStageState.Current => "bi-arrow-repeat",
            WorkflowStageState.Rejected => "bi-x-lg",
            _ => ""
        };

        public string StateLabel => State switch
        {
            WorkflowStageState.Completed => "مكتملة",
            WorkflowStageState.Current => "الحالية",
            WorkflowStageState.Rejected => "مرفوضة",
            _ => "قيد الانتظار"
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
