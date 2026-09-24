import { mockPrograms } from './mock-data';

export type Priority = 'high' | 'medium' | 'low';
export type NeedStatus = 'new' | 'in_progress' | 'completed';

export interface TrainingNeed {
  id: string;
  employeeName: string;
  employeeNumber: string;
  department: string;
  skillName: string;
  category: string;
  requiredLevel: number;
  currentLevel: number;
  status: NeedStatus;
}

export const trainingNeedDepartments = [
  'تقنية المعلومات',
  'الموارد البشرية',
  'المالية',
  'المبيعات',
  'التسويق',
  'خدمة العملاء',
  'الإدارة',
  'العمليات',
];

export const trainingNeedCategories = [
  'القيادة والإدارة',
  'إدارة المشاريع',
  'المهارات الشخصية',
  'تقنية المعلومات',
  'المالية والمحاسبة',
];

export const initialTrainingNeeds: TrainingNeed[] = [
  { id: 'n1', employeeName: 'أحمد المطيري', employeeNumber: 'EMP-1024', department: 'تقنية المعلومات', skillName: 'تحليل البيانات وPowerBI', category: 'تقنية المعلومات', requiredLevel: 5, currentLevel: 2, status: 'new' },
  { id: 'n2', employeeName: 'أحمد المطيري', employeeNumber: 'EMP-1024', department: 'تقنية المعلومات', skillName: 'الأمن السيبراني', category: 'تقنية المعلومات', requiredLevel: 4, currentLevel: 2, status: 'in_progress' },
  { id: 'n3', employeeName: 'سارة العتيبي', employeeNumber: 'EMP-2087', department: 'الموارد البشرية', skillName: 'إدارة الأداء', category: 'القيادة والإدارة', requiredLevel: 4, currentLevel: 3, status: 'new' },
  { id: 'n4', employeeName: 'سارة العتيبي', employeeNumber: 'EMP-2087', department: 'الموارد البشرية', skillName: 'مهارات التفاوض والتواصل', category: 'المهارات الشخصية', requiredLevel: 5, currentLevel: 2, status: 'new' },
  { id: 'n5', employeeName: 'خالد الدوسري', employeeNumber: 'EMP-3391', department: 'المالية', skillName: 'إدارة المشاريع PMP', category: 'إدارة المشاريع', requiredLevel: 5, currentLevel: 4, status: 'completed' },
  { id: 'n6', employeeName: 'خالد الدوسري', employeeNumber: 'EMP-3391', department: 'المالية', skillName: 'إدارة الوقت والإنتاجية', category: 'المهارات الشخصية', requiredLevel: 4, currentLevel: 2, status: 'new' },
];

export function getGap(need: TrainingNeed) {
  return Math.max(0, need.requiredLevel - need.currentLevel);
}

export function getPriority(gap: number): Priority {
  if (gap >= 3) return 'high';
  if (gap === 2) return 'medium';
  return 'low';
}

export const priorityMeta: Record<Priority, { label: string; className: string; dot: string }> = {
  high: { label: 'عالية', className: 'bg-red-500/10 text-red-600 border-red-200', dot: 'bg-red-500' },
  medium: { label: 'متوسطة', className: 'bg-amber-500/10 text-amber-600 border-amber-200', dot: 'bg-amber-500' },
  low: { label: 'منخفضة', className: 'bg-emerald-500/10 text-emerald-600 border-emerald-200', dot: 'bg-emerald-500' },
};

export const statusMeta: Record<NeedStatus, { label: string; className: string }> = {
  new: { label: 'جديد', className: 'bg-blue-500/10 text-blue-600 border-blue-200' },
  in_progress: { label: 'قيد المعالجة', className: 'bg-amber-500/10 text-amber-600 border-amber-200' },
  completed: { label: 'مكتمل', className: 'bg-emerald-500/10 text-emerald-600 border-emerald-200' },
};

// اقتراح البرنامج التدريبي الأنسب لسدّ الفجوة بمطابقة التصنيف ثم الكلمات المفتاحية
export function suggestProgram(need: TrainingNeed): { title: string; matched: boolean } {
  const active = mockPrograms.filter((p) => p.status === 'active');
  const byCategory = active.find((p) => (p.categories || []).includes(need.category));
  if (byCategory) return { title: byCategory.title, matched: true };
  const skill = need.skillName.toLowerCase();
  const byKeyword = active.find((p) =>
    skill.split(/\s+/).some((w) => w.length > 2 && p.title.toLowerCase().includes(w))
  );
  if (byKeyword) return { title: byKeyword.title, matched: true };
  return { title: 'لا يوجد برنامج مطابق — يُنصح بإضافة برنامج', matched: false };
}

export function computeReadiness(needs: TrainingNeed[]): number {
  if (needs.length === 0) return 100;
  return Math.round(
    (needs.reduce((sum, n) => sum + Math.min(n.currentLevel, n.requiredLevel) / n.requiredLevel, 0) /
      needs.length) *
      100
  );
}
