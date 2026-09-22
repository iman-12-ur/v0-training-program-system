// بيانات تجريبية واقعية لصفحة الاحتياجات التدريبية.
// عند الربط بواجهة API، يُستبدل هذا الملف بمصدر بيانات فعلي بنفس الأنواع.

import type {
  Employee,
  ImpactStage,
  JobTask,
  LearningPathStep,
  Skill,
  TrainingHistoryEntry,
} from "./types"

export const lastAssessmentDate = "22 سبتمبر 2026"

export const employee: Employee = {
  name: "أخصائي تحليل بيانات",
  jobTitle: "أخصائي تحليل بيانات",
  department: "دائرة التطوير المؤسسي",
  unit: "التدريب والتطوير",
  directManager: "محمد علي",
  autoFetched: true,
}

export const initialSkills: Skill[] = [
  { id: "power-bi", name: "Power BI", category: "practical-skill", requiredLevel: 4, currentLevel: 2 },
  { id: "ai", name: "الذكاء الاصطناعي", category: "technical-specialized", requiredLevel: 3, currentLevel: 1 },
  { id: "data-analysis", name: "تحليل البيانات", category: "practical-skill", requiredLevel: 4, currentLevel: 3 },
  { id: "reporting", name: "إعداد التقارير", category: "basic-knowledge", requiredLevel: 4, currentLevel: 4 },
  { id: "sql", name: "قواعد البيانات SQL", category: "technical-specialized", requiredLevel: 3, currentLevel: 2 },
  { id: "storytelling", name: "سرد البيانات", category: "weak-application", requiredLevel: 3, currentLevel: 2 },
  { id: "excel", name: "Excel المتقدم", category: "basic-knowledge", requiredLevel: 4, currentLevel: 4 },
  { id: "leadership", name: "قيادة الفرق التحليلية", category: "leadership", requiredLevel: 3, currentLevel: 3 },
]

export const initialTasks: JobTask[] = [
  {
    id: "task-1",
    name: "إعداد لوحات المعلومات التفاعلية",
    description: "بناء لوحات متابعة لمؤشرات الأداء باستخدام أدوات ذكاء الأعمال.",
    linkedSkillId: "power-bi",
    challenge: "high",
  },
  {
    id: "task-2",
    name: "تحليل مجموعات البيانات الكبيرة",
    description: "استخراج الأنماط والرؤى من بيانات الجهة لدعم القرار.",
    linkedSkillId: "data-analysis",
    challenge: "medium",
  },
  {
    id: "task-3",
    name: "توظيف نماذج الذكاء الاصطناعي",
    description: "استخدام النماذج التنبؤية لتحسين دقة التوقعات.",
    linkedSkillId: "ai",
    challenge: "high",
  },
  {
    id: "task-4",
    name: "إعداد التقارير الدورية",
    description: "تجهيز تقارير أداء واضحة لأصحاب العلاقة.",
    linkedSkillId: "reporting",
    challenge: "low",
  },
  {
    id: "task-5",
    name: "كتابة استعلامات قواعد البيانات",
    description: "استخراج البيانات من مصادر متعددة عبر استعلامات SQL.",
    linkedSkillId: "sql",
    challenge: "medium",
  },
]

export const trainingHistory: TrainingHistoryEntry[] = [
  {
    id: "th-1",
    program: "Power BI للمبتدئين",
    date: "15 مايو 2026",
    skillId: "power-bi",
    status: "completed",
    result: "partial",
    applied: "not-applied",
  },
  {
    id: "th-2",
    program: "أساسيات تحليل البيانات",
    date: "3 مارس 2026",
    skillId: "data-analysis",
    status: "completed",
    result: "improved",
    applied: "applied",
  },
  {
    id: "th-3",
    program: "مقدمة في الذكاء الاصطناعي",
    date: "20 يناير 2026",
    skillId: "ai",
    status: "completed",
    result: "no-change",
    applied: "not-applied",
  },
]

export const impactStages: ImpactStage[] = [
  { order: 1, title: "تجربة المتعلم وملاءمة المحتوى", description: "رضا المتعلم عن المحتوى وملاءمته للدور.", progress: 80 },
  { order: 2, title: "المعرفة / المهارة قبل وبعد", description: "مقارنة مستوى المعرفة قبل التدخل وبعده.", progress: 55 },
  { order: 3, title: "تطبيق المهارة 30 / 60 / 90 يوماً", description: "قياس مدى تطبيق المهارة في بيئة العمل.", progress: 30 },
  { order: 4, title: "الأثر على المهمة أو مؤشر الأداء", description: "الأثر النهائي على مؤشرات الأداء.", progress: 15 },
]

// المسار المقترح الأولي — يُعاد بناؤه ديناميكياً بعد تحليل الفجوات.
export const baseLearningPath: LearningPathStep[] = [
  { order: 1, title: "تقييم تشخيصي", status: "completed", kind: "diagnostic" },
  { order: 2, title: "محتوى تمهيدي قصير", status: "proposed", kind: "microlearning" },
  { order: 3, title: "تدريب تطبيقي على Power BI", status: "proposed", kind: "applied-training" },
  { order: 4, title: "مشروع تطبيقي: إنشاء لوحة معلومات", status: "proposed", kind: "project" },
  { order: 5, title: "تطبيق في بيئة العمل", status: "upcoming", kind: "practice" },
  { order: 6, title: "قياس بعد 30 يوماً", status: "upcoming", kind: "measure" },
  { order: 7, title: "قياس بعد 60 يوماً", status: "upcoming", kind: "measure" },
  { order: 8, title: "قياس بعد 90 يوماً", status: "upcoming", kind: "measure" },
]
