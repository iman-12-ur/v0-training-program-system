// أنواع البيانات لصفحة الاحتياجات التدريبية في منظومة "مساري".
// الهيكل مصمم ليكون جاهزاً لاستبدال البيانات التجريبية بواجهة API لاحقاً.

export type ChallengeLevel = "none" | "low" | "medium" | "high"

export type ObstacleType =
  | "knowledge"
  | "skill"
  | "practice"
  | "system"
  | "resources"
  | "unknown"

export type SkillPriority = "high" | "medium" | "low"

export type SkillStatus = "gap" | "complete"

// أنواع التشخيص الأولي لسبب الفجوة.
export type GapDiagnosis =
  | "insufficient-knowledge"
  | "incomplete-skill"
  | "weak-application"
  | "lack-experience"
  | "tool-system"
  | "unclear"

// فئة الفجوة التي يعتمد عليها محرك التوصية.
export type GapCategory =
  | "basic-knowledge"
  | "practical-skill"
  | "weak-application"
  | "needs-experience"
  | "leadership"
  | "technical-specialized"
  | "unclear"

export type InterventionType =
  | "elearning"
  | "microlearning"
  | "applied-training"
  | "practice"
  | "coaching"
  | "mentoring"
  | "project"
  | "job-rotation"
  | "progressive-path"
  | "diagnostic"

export type LearningPreference =
  | "elearning"
  | "applied-training"
  | "practice"
  | "coaching"
  | "mentoring"
  | "project"
  | "job-rotation"

export interface Employee {
  name: string
  jobTitle: string
  department: string
  unit: string
  directManager: string
  autoFetched: boolean
}

export interface JobTask {
  id: string
  name: string
  description: string
  linkedSkillId: string
  challenge: ChallengeLevel
}

export interface Skill {
  id: string
  name: string
  category: GapCategory
  requiredLevel: number // 1..5
  currentLevel: number // 1..5
}

export interface TrainingHistoryEntry {
  id: string
  program: string
  date: string
  skillId: string
  status: "completed" | "in-progress" | "not-started"
  result: "improved" | "partial" | "no-change"
  applied: "applied" | "partial" | "not-applied"
}

export interface LearningPathStep {
  order: number
  title: string
  description?: string
  status: "completed" | "proposed" | "upcoming"
  kind: InterventionType | "measure"
}

export interface ImpactStage {
  order: number
  title: string
  description?: string
  progress: number // 0..100
}

// نتيجة محسوبة لكل مهارة بعد تطبيق منطق الفجوة والأولوية والتوصية.
export interface SkillAnalysis {
  skill: Skill
  gap: number
  priority: SkillPriority
  status: SkillStatus
  diagnosisLabel: string
  intervention: string
  interventionReason: string
}
