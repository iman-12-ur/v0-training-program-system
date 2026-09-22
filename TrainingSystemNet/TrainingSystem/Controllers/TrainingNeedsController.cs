using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TrainingSystem.Controllers
{
    // صفحة تشخيص الاحتياجات التدريبية — رحلة تشخيصية (Problem First → Solution Second).
    // بيانات تجريبية ثابتة ومنطق محسوب في الذاكرة فقط — لا تتعامل مع قاعدة البيانات إطلاقاً.
    [Authorize(Roles = "SuperAdmin,Admin,Supervisor")]
    public class TrainingNeedsController : Controller
    {
        public IActionResult Index()
        {
            var model = TrainingNeedsData.Build();
            return View(model);
        }
    }

    #region Enums & Helpers

    public enum SkillCategory
    {
        Technical,   // تقنية
        Behavioral,  // سلوكية
        Leadership   // قيادية
    }

    public enum GapPriority
    {
        None,     // لا فجوة
        Low,      // منخفضة
        Medium,   // متوسطة
        High      // عالية
    }

    public static class TrainingNeedsHelper
    {
        public static readonly string[] LevelLabels =
        {
            "", "مبتدئ", "أساسي", "متوسط", "متقدم", "خبير"
        };

        public static string LevelLabel(int level)
            => level >= 1 && level <= 5 ? LevelLabels[level] : "-";

        public static string CategoryLabel(SkillCategory category) => category switch
        {
            SkillCategory.Technical => "مهارة تقنية",
            SkillCategory.Behavioral => "مهارة سلوكية",
            SkillCategory.Leadership => "مهارة قيادية",
            _ => "مهارة"
        };

        public static int Gap(int required, int current) => Math.Max(0, required - current);

        public static GapPriority Priority(int required, int current)
        {
            var gap = Gap(required, current);
            if (gap <= 0) return GapPriority.None;
            if (gap == 1) return GapPriority.Low;
            if (gap == 2) return GapPriority.Medium;
            return GapPriority.High;
        }

        public static string PriorityLabel(GapPriority p) => p switch
        {
            GapPriority.None => "متقن",
            GapPriority.Low => "أولوية منخفضة",
            GapPriority.Medium => "أولوية متوسطة",
            GapPriority.High => "أولوية عالية",
            _ => ""
        };

        // اسم لون Bootstrap المناسب للأولوية
        public static string PriorityColor(GapPriority p) => p switch
        {
            GapPriority.None => "success",
            GapPriority.Low => "info",
            GapPriority.Medium => "warning",
            GapPriority.High => "danger",
            _ => "secondary"
        };

        // نسبة الجاهزية الكلية = متوسط (الحالي/المطلوب) لكل المهارات
        public static int Readiness(IEnumerable<SkillAssessment> skills)
        {
            var list = skills.ToList();
            if (list.Count == 0) return 0;
            double sum = list.Sum(s => Math.Min(1.0, (double)s.CurrentLevel / Math.Max(1, s.RequiredLevel)));
            return (int)Math.Round(sum / list.Count * 100);
        }
    }

    #endregion

    #region View Models

    public class TrainingNeedsViewModel
    {
        public EmployeeProfile Employee { get; set; } = new();
        public List<JobTaskItem> Tasks { get; set; } = new();
        public List<SkillAssessment> Skills { get; set; } = new();
        public List<EmployeeVoiceItem> Voice { get; set; } = new();
        public List<PreviousLearningItem> PreviousLearning { get; set; } = new();
        public List<LearningPathStep> LearningPath { get; set; } = new();
        public List<ImpactMetric> ImpactMetrics { get; set; } = new();

        public int TotalSkills => Skills.Count;
        public int MasteredSkills => Skills.Count(s => TrainingNeedsHelper.Gap(s.RequiredLevel, s.CurrentLevel) == 0);
        public int HighGaps => Skills.Count(s => TrainingNeedsHelper.Priority(s.RequiredLevel, s.CurrentLevel) == GapPriority.High);
        public int MediumGaps => Skills.Count(s => TrainingNeedsHelper.Priority(s.RequiredLevel, s.CurrentLevel) == GapPriority.Medium);
        public int LowGaps => Skills.Count(s => TrainingNeedsHelper.Priority(s.RequiredLevel, s.CurrentLevel) == GapPriority.Low);
        public int Readiness => TrainingNeedsHelper.Readiness(Skills);
    }

    public class EmployeeProfile
    {
        public string Name { get; set; } = "";
        public string JobTitle { get; set; } = "";
        public string Department { get; set; } = "";
        public string EmployeeId { get; set; } = "";
        public string Grade { get; set; } = "";
        public int YearsInRole { get; set; }
        public string Manager { get; set; } = "";
        public DateTime LastAssessment { get; set; }
    }

    public class JobTaskItem
    {
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        // مستوى التحدي 1..5
        public int ChallengeLevel { get; set; }
        // العائق الرئيسي أمام إتقان المهمة
        public string Obstacle { get; set; } = "";
    }

    public class SkillAssessment
    {
        public string Name { get; set; } = "";
        public SkillCategory Category { get; set; }
        public int RequiredLevel { get; set; }
        public int CurrentLevel { get; set; }
        // سبب الفجوة (يظهر عند وجود فجوة)
        public string Cause { get; set; } = "";

        public int Gap => TrainingNeedsHelper.Gap(RequiredLevel, CurrentLevel);
        public GapPriority Priority => TrainingNeedsHelper.Priority(RequiredLevel, CurrentLevel);

        // التوصية المقترحة بناءً على نوع المهارة وحجم الفجوة
        public string Recommendation
        {
            get
            {
                if (Gap <= 0) return "الحفاظ على المستوى عبر مهام متقدمة والتعلّم الذاتي المستمر.";
                return Category switch
                {
                    SkillCategory.Leadership when Gap >= 2 => "برنامج تطوير قيادي مكثّف مع إرشاد فردي (Coaching) من قيادي أعلى.",
                    SkillCategory.Leadership => "ورشة قيادية موجّهة + تكليف بمهام إشرافية متدرجة.",
                    SkillCategory.Behavioral when Gap >= 2 => "برنامج سلوكي عملي + جلسات إرشاد وتغذية راجعة دورية.",
                    SkillCategory.Behavioral => "ورشة عمل تطبيقية قصيرة مع ممارسة موجّهة على أرض العمل.",
                    SkillCategory.Technical when Gap >= 2 => "برنامج تدريبي تقني مكثّف مع مشروع تطبيقي وتقييم عملي.",
                    _ => "تدريب تقني موجّه (ورشة) + مهمة تطبيقية مصغّرة للتثبيت."
                };
            }
        }

        public string InterventionType
        {
            get
            {
                if (Gap <= 0) return "تعلّم ذاتي";
                if (Gap >= 3) return "برنامج مكثّف";
                if (Gap == 2) return Category == SkillCategory.Behavioral ? "إرشاد + ورشة" : "برنامج تدريبي";
                return "ورشة عمل";
            }
        }
    }

    public class EmployeeVoiceItem
    {
        public string Question { get; set; } = "";
        public string Answer { get; set; } = "";
        public string Icon { get; set; } = "bi-chat-quote";
    }

    public class PreviousLearningItem
    {
        public string Title { get; set; } = "";
        public string Type { get; set; } = "";
        public DateTime CompletedAt { get; set; }
        public int Score { get; set; }
        public bool Applied { get; set; }
    }

    public class LearningPathStep
    {
        public int Order { get; set; }
        public string Title { get; set; } = "";
        public string Type { get; set; } = "";
        public string Duration { get; set; } = "";
        public string TargetSkill { get; set; } = "";
        public GapPriority Priority { get; set; }
    }

    public class ImpactMetric
    {
        public string Stage { get; set; } = "";      // 30 / 60 / 90 يوم
        public string Metric { get; set; } = "";
        public string Target { get; set; } = "";
    }

    #endregion

    #region Mock Data

    public static class TrainingNeedsData
    {
        public static TrainingNeedsViewModel Build()
        {
            return new TrainingNeedsViewModel
            {
                Employee = new EmployeeProfile
                {
                    Name = "أحمد بن سالم العامري",
                    JobTitle = "مصمم منتجات أول",
                    Department = "إدارة التطوير والتصميم",
                    EmployeeId = "EMP-20481",
                    Grade = "الدرجة الثامنة",
                    YearsInRole = 3,
                    Manager = "م. هدى بنت خالد الرشيدي",
                    LastAssessment = DateTime.Now.AddDays(-12)
                },
                Tasks = new List<JobTaskItem>
                {
                    new() { Title = "قيادة أبحاث تجربة المستخدم", Description = "تخطيط وإجراء مقابلات واختبارات قابلية الاستخدام وتحليل نتائجها.", ChallengeLevel = 4, Obstacle = "نقص في أدوات التحليل الكمي للبيانات" },
                    new() { Title = "تصميم أنظمة تصميم متكاملة", Description = "بناء وصيانة Design System موحّد عبر المنتجات.", ChallengeLevel = 3, Obstacle = "غياب معايير موثّقة للحوكمة" },
                    new() { Title = "توجيه المصممين المبتدئين", Description = "مراجعة الأعمال وتقديم تغذية راجعة وإرشاد الفريق.", ChallengeLevel = 5, Obstacle = "قلة الخبرة في الإرشاد المنظّم" },
                    new() { Title = "عرض الحلول على أصحاب المصلحة", Description = "تقديم المقترحات وإقناع الجهات المعنية بالقرارات التصميمية.", ChallengeLevel = 4, Obstacle = "ضعف في بناء السرد المقنع بالبيانات" },
                },
                Skills = new List<SkillAssessment>
                {
                    new() { Name = "تصميم تجربة المستخدم (UX)", Category = SkillCategory.Technical, RequiredLevel = 5, CurrentLevel = 4, Cause = "خبرة قوية لكن تنقص الممارسة في الأنظمة المعقّدة." },
                    new() { Name = "أبحاث المستخدم وتحليلها", Category = SkillCategory.Technical, RequiredLevel = 5, CurrentLevel = 3, Cause = "غياب تدريب على الأساليب الكمية وأدوات التحليل." },
                    new() { Name = "تحليل البيانات وPowerBI", Category = SkillCategory.Technical, RequiredLevel = 5, CurrentLevel = 2, Cause = "لم يخضع لأي تدريب رسمي على تحليل البيانات." },
                    new() { Name = "إتقان Figma المتقدم", Category = SkillCategory.Technical, RequiredLevel = 4, CurrentLevel = 4, Cause = "" },
                    new() { Name = "القيادة وإدارة الفريق", Category = SkillCategory.Leadership, RequiredLevel = 4, CurrentLevel = 2, Cause = "انتقل حديثاً لدور فيه مسؤوليات إشرافية دون إعداد قيادي." },
                    new() { Name = "إدارة أصحاب المصلحة", Category = SkillCategory.Leadership, RequiredLevel = 4, CurrentLevel = 3, Cause = "يحتاج ممارسة أكبر في التفاوض والإقناع." },
                    new() { Name = "التواصل والعرض", Category = SkillCategory.Behavioral, RequiredLevel = 4, CurrentLevel = 3, Cause = "أداء جيد لكن يفتقر لبناء السرد بالبيانات." },
                    new() { Name = "إدارة الوقت والأولويات", Category = SkillCategory.Behavioral, RequiredLevel = 4, CurrentLevel = 4, Cause = "" },
                },
                Voice = new List<EmployeeVoiceItem>
                {
                    new() { Question = "ما أكثر مهمة تستهلك وقتك دون نتيجة مُرضية؟", Answer = "تحليل نتائج الأبحاث يدوياً بسبب ضعف مهارتي في أدوات التحليل الكمي.", Icon = "bi-hourglass-split" },
                    new() { Question = "أين تشعر أنك بحاجة لدعم لتؤدي دورك الجديد؟", Answer = "في قيادة الفريق وتقديم التغذية الراجعة البنّاءة للمصممين المبتدئين.", Icon = "bi-life-preserver" },
                    new() { Question = "ما المهارة التي لو أتقنتها لأحدثت أكبر فرق؟", Answer = "تحليل البيانات — سيمكّنني من اتخاذ قرارات تصميمية مدعومة بالأدلة.", Icon = "bi-stars" },
                },
                PreviousLearning = new List<PreviousLearningItem>
                {
                    new() { Title = "أساسيات تجربة المستخدم", Type = "برنامج تدريبي", CompletedAt = DateTime.Now.AddMonths(-14), Score = 92, Applied = true },
                    new() { Title = "ورشة Design Thinking", Type = "ورشة عمل", CompletedAt = DateTime.Now.AddMonths(-8), Score = 88, Applied = true },
                    new() { Title = "مقدمة في القيادة", Type = "دورة قصيرة", CompletedAt = DateTime.Now.AddMonths(-3), Score = 76, Applied = false },
                },
                LearningPath = new List<LearningPathStep>
                {
                    new() { Order = 1, Title = "برنامج تحليل البيانات وPowerBI للمصممين", Type = "برنامج مكثّف", Duration = "4 أسابيع", TargetSkill = "تحليل البيانات وPowerBI", Priority = GapPriority.High },
                    new() { Order = 2, Title = "برنامج الإعداد القيادي للمشرفين الجدد", Type = "برنامج + إرشاد", Duration = "6 أسابيع", TargetSkill = "القيادة وإدارة الفريق", Priority = GapPriority.High },
                    new() { Order = 3, Title = "ورشة أساليب البحث الكمي", Type = "ورشة عمل", Duration = "أسبوعان", TargetSkill = "أبحاث المستخدم وتحليلها", Priority = GapPriority.Medium },
                    new() { Order = 4, Title = "ورشة السرد المقنع بالبيانات", Type = "ورشة عمل", Duration = "3 أيام", TargetSkill = "التواصل والعرض", Priority = GapPriority.Low },
                },
                ImpactMetrics = new List<ImpactMetric>
                {
                    new() { Stage = "بعد 30 يوم", Metric = "تطبيق مهارة تحليل البيانات في تقرير بحثي واحد", Target = "تقرير مكتمل بلوحة PowerBI" },
                    new() { Stage = "بعد 60 يوم", Metric = "قيادة جلسة تغذية راجعة منظّمة للفريق", Target = "جلستان موثّقتان" },
                    new() { Stage = "بعد 90 يوم", Metric = "رفع مستوى الجاهزية الكلية", Target = "من 73% إلى 88%+" },
                },
            };
        }
    }

    #endregion
}
