// منطق التشخيص: حساب الفجوة، تحديد الأولوية، ومحرك التوصية.
// كل دالة نقية (pure) لتسهيل الاختبار وإعادة الاستخدام مع بيانات فعلية لاحقاً.

import type {
  GapCategory,
  LearningPreference,
  Skill,
  SkillAnalysis,
  SkillPriority,
  TrainingHistoryEntry,
} from "./types"

export const SKILL_LEVELS = [
  { value: 1, label: "مبتدئ" },
  { value: 2, label: "أساسي" },
  { value: 3, label: "متوسط" },
  { value: 4, label: "متقدم" },
  { value: 5, label: "متمكن" },
] as const

export function skillLevelLabel(level: number): string {
  return SKILL_LEVELS.find((l) => l.value === level)?.label ?? "غير محدد"
}

// Gap = Required - Current، ولا تقل عن صفر.
export function calculateGap(skill: Pick<Skill, "requiredLevel" | "currentLevel">): number {
  return Math.max(0, skill.requiredLevel - skill.currentLevel)
}

// الأولوية مشتقة من حجم الفجوة.
export function determinePriority(gap: number): SkillPriority {
  if (gap >= 2) return "high"
  if (gap === 1) return "medium"
  return "low"
}

export const priorityLabels: Record<SkillPriority, string> = {
  high: "عالية",
  medium: "متوسطة",
  low: "منخفضة",
}

// تسمية عربية لفئة الفجوة (نوع الفجوة).
export const gapCategoryLabels: Record<GapCategory, string> = {
  "basic-knowledge": "معرفة أساسية",
  "practical-skill": "مهارة عملية",
  "weak-application": "ضعف في التطبيق",
  "needs-experience": "حاجة إلى خبرة عملية",
  leadership: "مهارة قيادية",
  "technical-specialized": "مهارة تقنية متخصصة",
  unclear: "فجوة غير واضحة",
}

// محرك التوصية: يربط فئة الفجوة بالتدخل المناسب وسببه (قاعدة العمل رقم 5).
export function recommendIntervention(category: GapCategory): {
  intervention: string
  reason: string
} {
  switch (category) {
    case "basic-knowledge":
      return {
        intervention: "تعلم إلكتروني / محتوى قصير",
        reason: "لأن الفجوة مرتبطة بمعرفة أساسية يمكن سدها عبر محتوى تعليمي موجه.",
      }
    case "practical-skill":
      return {
        intervention: "تدريب تطبيقي + ممارسة",
        reason: "لأن الفجوة مرتبطة بمهارة عملية تحتاج إلى ممارسة وتطبيق.",
      }
    case "weak-application":
      return {
        intervention: "Coaching / Mentoring",
        reason: "لأن المعرفة متوفرة لكن التطبيق ضعيف ويحتاج إلى توجيه ومتابعة.",
      }
    case "needs-experience":
      return {
        intervention: "مشروع تطبيقي / Job Rotation",
        reason: "لأن الفجوة تتطلب خبرة عملية تُكتسب من مواقف حقيقية.",
      }
    case "leadership":
      return {
        intervention: "Coaching + برنامج قيادي",
        reason: "لأن المهارة قيادية وتتطور عبر التوجيه والبرامج المتخصصة.",
      }
    case "technical-specialized":
      return {
        intervention: "مسار متدرج + شهادة عند الحاجة",
        reason: "لأن المهارة تقنية متخصصة وتحتاج إلى مسار متدرج مع اعتماد مهني.",
      }
    case "unclear":
    default:
      return {
        intervention: "تقييم تشخيصي إضافي",
        reason: "لأن سبب الفجوة غير واضح ويحتاج إلى تشخيص أعمق قبل تحديد التدخل.",
      }
  }
}

// تشخيص أولي نصّي مشتق من فئة الفجوة (يظهر في تحليل سبب الفجوة).
export const diagnosisByCategory: Record<GapCategory, string> = {
  "basic-knowledge": "معرفة غير كافية",
  "practical-skill": "مهارة غير مكتملة",
  "weak-application": "ضعف في التطبيق",
  "needs-experience": "قلة الخبرة العملية",
  leadership: "تحتاج إلى تطوير قيادي",
  "technical-specialized": "أداة / نظام متخصص",
  unclear: "غير واضح",
}

// تحويل مهارة خام إلى تحليل كامل جاهز للعرض.
export function analyzeSkill(skill: Skill): SkillAnalysis {
  const gap = calculateGap(skill)
  const priority = determinePriority(gap)
  const { intervention, reason } = recommendIntervention(skill.category)
  return {
    skill,
    gap,
    priority,
    status: gap === 0 ? "complete" : "gap",
    diagnosisLabel: gap === 0 ? "مكتملة" : diagnosisByCategory[skill.category],
    intervention,
    interventionReason: reason,
  }
}

export interface GapSummary {
  totalSkills: number
  highGaps: number
  mediumGaps: number
  completed: number
}

export function summarizeGaps(analyses: SkillAnalysis[]): GapSummary {
  return {
    totalSkills: analyses.length,
    highGaps: analyses.filter((a) => a.status === "gap" && a.priority === "high").length,
    mediumGaps: analyses.filter((a) => a.status === "gap" && a.priority === "medium").length,
    completed: analyses.filter((a) => a.status === "complete").length,
  }
}

// مؤشر جاهزية المهارات: نسبة مجموع المستويات الحالية إلى المطلوبة (مؤشر تجريبي).
export function calculateReadiness(skills: Skill[]): number {
  const required = skills.reduce((sum, s) => sum + s.requiredLevel, 0)
  if (required === 0) return 0
  const current = skills.reduce((sum, s) => sum + Math.min(s.currentLevel, s.requiredLevel), 0)
  return Math.round((current / required) * 100)
}

// استنتاج توصية إضافية من سجل التعلم السابق (قواعد العمل 6 و7).
export function historyInsight(entry: TrainingHistoryEntry): {
  tone: "success" | "warning" | "danger"
  text: string
} {
  if (entry.result !== "no-change" && entry.applied !== "not-applied" && entry.result === "improved") {
    return { tone: "success", text: "تحسّن مطبّق — يمكن الانتقال إلى مستوى متقدم عند الحاجة." }
  }
  if (entry.status === "completed" && entry.applied === "not-applied") {
    return { tone: "warning", text: "تم التدريب دون تطبيق — يوصى بتدخل عملي ومتابعة المدير." }
  }
  if (entry.status === "completed" && entry.result === "no-change") {
    return { tone: "danger", text: "الفجوة قائمة رغم التدريب — يلزم تحليل السبب وتدخل مختلف." }
  }
  return { tone: "warning", text: "بحاجة إلى متابعة لإغلاق الفجوة." }
}

export const learningPreferenceOptions: { value: LearningPreference; label: string }[] = [
  { value: "elearning", label: "تعلم إلكتروني" },
  { value: "applied-training", label: "تدريب تطبيقي" },
  { value: "practice", label: "ممارسة عملية" },
  { value: "coaching", label: "Coaching" },
  { value: "mentoring", label: "Mentoring" },
  { value: "project", label: "مشروع تطبيقي" },
  { value: "job-rotation", label: "Job Rotation" },
]
