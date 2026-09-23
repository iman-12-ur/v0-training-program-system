'use client';

import { useMemo, useState } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from '@/components/ui/dialog';
import {
  ClipboardPlus,
  Plus,
  Filter,
  AlertTriangle,
  Gauge,
  Trash2,
  TrendingUp,
} from 'lucide-react';

type Priority = 'high' | 'medium' | 'low';
type NeedStatus = 'new' | 'in_progress' | 'completed';

interface TrainingNeed {
  id: string;
  employeeName: string;
  employeeNumber: string;
  department: string;
  skillName: string;
  category: string;
  requiredLevel: number;
  currentLevel: number;
}

// بيانات تجريبية توضيحية (تطابق ما في نظام .NET) — تُستبدل بقاعدة البيانات في التطبيق الفعلي
const initialNeeds: TrainingNeed[] = [
  { id: 'n1', employeeName: 'أحمد المطيري', employeeNumber: 'EMP-1024', department: 'تقنية المعلومات', skillName: 'تحليل البيانات وPowerBI', category: 'تقنية', requiredLevel: 5, currentLevel: 2 },
  { id: 'n2', employeeName: 'أحمد المطيري', employeeNumber: 'EMP-1024', department: 'تقنية المعلومات', skillName: 'أمن المعلومات', category: 'تقنية', requiredLevel: 4, currentLevel: 2 },
  { id: 'n3', employeeName: 'سارة العتيبي', employeeNumber: 'EMP-2087', department: 'الموارد البشرية', skillName: 'إدارة الأداء', category: 'إدارية', requiredLevel: 4, currentLevel: 3 },
  { id: 'n4', employeeName: 'سارة العتيبي', employeeNumber: 'EMP-2087', department: 'الموارد البشرية', skillName: 'مهارات التفاوض', category: 'سلوكية', requiredLevel: 5, currentLevel: 2 },
  { id: 'n5', employeeName: 'خالد الدوسري', employeeNumber: 'EMP-3391', department: 'المالية', skillName: 'المعايير المحاسبية الدولية', category: 'تقنية', requiredLevel: 5, currentLevel: 4 },
  { id: 'n6', employeeName: 'خالد الدوسري', employeeNumber: 'EMP-3391', department: 'المالية', skillName: 'إعداد الميزانيات', category: 'إدارية', requiredLevel: 4, currentLevel: 2 },
];

function getGap(need: TrainingNeed) {
  return Math.max(0, need.requiredLevel - need.currentLevel);
}

function getPriority(gap: number): Priority {
  if (gap >= 3) return 'high';
  if (gap === 2) return 'medium';
  return 'low';
}

const priorityMeta: Record<Priority, { label: string; className: string }> = {
  high: { label: 'عالية', className: 'bg-red-500/10 text-red-600 border-red-200' },
  medium: { label: 'متوسطة', className: 'bg-amber-500/10 text-amber-600 border-amber-200' },
  low: { label: 'منخفضة', className: 'bg-emerald-500/10 text-emerald-600 border-emerald-200' },
};

const departments = ['الكل', 'تقنية المعلومات', 'الموارد البشرية', 'المالية'];
const categories = ['تقنية', 'إدارية', 'سلوكية'];

export function TrainingNeedsView() {
  const [needs, setNeeds] = useState<TrainingNeed[]>(initialNeeds);
  const [departmentFilter, setDepartmentFilter] = useState('الكل');
  const [isAddOpen, setIsAddOpen] = useState(false);
  const [newNeed, setNewNeed] = useState({
    employeeName: '',
    employeeNumber: '',
    department: 'تقنية المعلومات',
    skillName: '',
    category: 'تقنية',
    requiredLevel: 4,
    currentLevel: 2,
  });

  const filteredNeeds = useMemo(
    () =>
      departmentFilter === 'الكل'
        ? needs
        : needs.filter((n) => n.department === departmentFilter),
    [needs, departmentFilter]
  );

  const stats = useMemo(() => {
    const total = filteredNeeds.length;
    const highPriority = filteredNeeds.filter((n) => getPriority(getGap(n)) === 'high').length;
    const readiness =
      total === 0
        ? 100
        : Math.round(
            (filteredNeeds.reduce(
              (sum, n) => sum + Math.min(n.currentLevel, n.requiredLevel) / n.requiredLevel,
              0
            ) /
              total) *
              100
          );
    return { total, highPriority, readiness };
  }, [filteredNeeds]);

  const updateCurrentLevel = (id: string, level: number) => {
    setNeeds((prev) =>
      prev.map((n) => (n.id === id ? { ...n, currentLevel: level } : n))
    );
  };

  const deleteNeed = (id: string) => {
    setNeeds((prev) => prev.filter((n) => n.id !== id));
  };

  const addNeed = () => {
    if (!newNeed.employeeName.trim() || !newNeed.skillName.trim()) return;
    setNeeds((prev) => [
      {
        id: `n-${Date.now()}`,
        ...newNeed,
      },
      ...prev,
    ]);
    setIsAddOpen(false);
    setNewNeed({
      employeeName: '',
      employeeNumber: '',
      department: 'تقنية المعلومات',
      skillName: '',
      category: 'تقنية',
      requiredLevel: 4,
      currentLevel: 2,
    });
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h2 className="text-2xl font-bold text-foreground">الاحتياجات التدريبية</h2>
          <p className="text-muted-foreground">
            تحديد الفجوات المهارية للموظفين حسب الدائرة واقتراح التدريب المناسب
          </p>
        </div>
        <Button onClick={() => setIsAddOpen(true)}>
          <Plus className="ml-2 h-4 w-4" />
          إضافة احتياج
        </Button>
      </div>

      {/* Stats */}
      <div className="grid gap-4 sm:grid-cols-3">
        <Card className="border-none shadow-sm">
          <CardContent className="flex items-center gap-4 p-6">
            <div className="rounded-full bg-blue-500/10 p-3">
              <ClipboardPlus className="h-6 w-6 text-blue-600" />
            </div>
            <div>
              <p className="text-2xl font-bold text-foreground">{stats.total}</p>
              <p className="text-sm text-muted-foreground">إجمالي الاحتياجات</p>
            </div>
          </CardContent>
        </Card>

        <Card className="border-none shadow-sm">
          <CardContent className="flex items-center gap-4 p-6">
            <div className="rounded-full bg-red-500/10 p-3">
              <AlertTriangle className="h-6 w-6 text-red-600" />
            </div>
            <div>
              <p className="text-2xl font-bold text-foreground">{stats.highPriority}</p>
              <p className="text-sm text-muted-foreground">فجوات ذات أولوية عالية</p>
            </div>
          </CardContent>
        </Card>

        <Card className="border-none shadow-sm">
          <CardContent className="flex items-center gap-4 p-6">
            <div className="rounded-full bg-emerald-500/10 p-3">
              <Gauge className="h-6 w-6 text-emerald-600" />
            </div>
            <div>
              <p className="text-2xl font-bold text-foreground">{stats.readiness}%</p>
              <p className="text-sm text-muted-foreground">مؤشر الجاهزية الكلية</p>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Filter */}
      <div className="flex items-center gap-3">
        <Select value={departmentFilter} onValueChange={setDepartmentFilter}>
          <SelectTrigger className="w-full sm:w-56">
            <Filter className="ml-2 h-4 w-4" />
            <SelectValue placeholder="الدائرة" />
          </SelectTrigger>
          <SelectContent>
            {departments.map((d) => (
              <SelectItem key={d} value={d}>
                {d}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
        <Badge variant="secondary">{filteredNeeds.length} سجل</Badge>
      </div>

      {/* Table */}
      <Card className="border-none shadow-sm">
        <CardHeader>
          <CardTitle className="flex items-center gap-2 text-lg">
            <TrendingUp className="h-5 w-5 text-primary" />
            تقييم المهارات وتحليل الفجوات
          </CardTitle>
        </CardHeader>
        <CardContent>
          {filteredNeeds.length === 0 ? (
            <div className="flex flex-col items-center justify-center py-12 text-center">
              <ClipboardPlus className="h-10 w-10 text-muted-foreground" />
              <p className="mt-3 text-muted-foreground">لا توجد احتياجات مسجّلة لهذه الدائرة</p>
            </div>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full text-right text-sm">
                <thead>
                  <tr className="border-b text-muted-foreground">
                    <th className="p-3 font-medium">الموظف</th>
                    <th className="p-3 font-medium">الدائرة</th>
                    <th className="p-3 font-medium">المهارة</th>
                    <th className="p-3 font-medium">المطلوب</th>
                    <th className="p-3 font-medium">الحالي (اضغط لتعديله)</th>
                    <th className="p-3 font-medium">الفجوة</th>
                    <th className="p-3 font-medium">الأولوية</th>
                    <th className="p-3 font-medium"></th>
                  </tr>
                </thead>
                <tbody>
                  {filteredNeeds.map((need) => {
                    const gap = getGap(need);
                    const priority = getPriority(gap);
                    const meta = priorityMeta[priority];
                    return (
                      <tr key={need.id} className="border-b last:border-0 hover:bg-muted/40">
                        <td className="p-3">
                          <div className="font-medium text-foreground">{need.employeeName}</div>
                          <div className="text-xs text-muted-foreground">{need.employeeNumber}</div>
                        </td>
                        <td className="p-3 text-muted-foreground">{need.department}</td>
                        <td className="p-3">
                          <div className="font-medium text-foreground">{need.skillName}</div>
                          <div className="text-xs text-muted-foreground">{need.category}</div>
                        </td>
                        <td className="p-3 font-semibold text-foreground">{need.requiredLevel}</td>
                        <td className="p-3">
                          <div className="flex gap-1" dir="rtl">
                            {[1, 2, 3, 4, 5].map((lvl) => (
                              <button
                                key={lvl}
                                type="button"
                                aria-label={`المستوى ${lvl}`}
                                onClick={() => updateCurrentLevel(need.id, lvl)}
                                className={
                                  'h-4 w-7 rounded border transition-colors ' +
                                  (lvl <= need.currentLevel
                                    ? 'border-primary bg-primary'
                                    : 'border-border bg-muted hover:border-primary')
                                }
                              />
                            ))}
                          </div>
                        </td>
                        <td className="p-3 font-semibold text-foreground">{gap}</td>
                        <td className="p-3">
                          <Badge variant="outline" className={meta.className}>
                            {meta.label}
                          </Badge>
                        </td>
                        <td className="p-3">
                          <Button
                            variant="ghost"
                            size="icon"
                            className="h-8 w-8 text-muted-foreground hover:text-red-600"
                            onClick={() => deleteNeed(need.id)}
                          >
                            <Trash2 className="h-4 w-4" />
                          </Button>
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Add dialog */}
      <Dialog open={isAddOpen} onOpenChange={setIsAddOpen}>
        <DialogContent style={{ direction: 'rtl' }}>
          <DialogHeader>
            <DialogTitle>إضافة احتياج تدريبي</DialogTitle>
          </DialogHeader>
          <div className="grid gap-4 py-2">
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label>اسم الموظف</Label>
                <Input
                  value={newNeed.employeeName}
                  onChange={(e) => setNewNeed((p) => ({ ...p, employeeName: e.target.value }))}
                  className="text-right"
                />
              </div>
              <div className="space-y-2">
                <Label>الرقم الوظيفي</Label>
                <Input
                  value={newNeed.employeeNumber}
                  onChange={(e) => setNewNeed((p) => ({ ...p, employeeNumber: e.target.value }))}
                  className="text-right"
                />
              </div>
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label>الدائرة</Label>
                <Select
                  value={newNeed.department}
                  onValueChange={(v) => setNewNeed((p) => ({ ...p, department: v }))}
                >
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    {departments
                      .filter((d) => d !== 'الكل')
                      .map((d) => (
                        <SelectItem key={d} value={d}>
                          {d}
                        </SelectItem>
                      ))}
                  </SelectContent>
                </Select>
              </div>
              <div className="space-y-2">
                <Label>التصنيف</Label>
                <Select
                  value={newNeed.category}
                  onValueChange={(v) => setNewNeed((p) => ({ ...p, category: v }))}
                >
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    {categories.map((c) => (
                      <SelectItem key={c} value={c}>
                        {c}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
            </div>
            <div className="space-y-2">
              <Label>المهارة</Label>
              <Input
                value={newNeed.skillName}
                onChange={(e) => setNewNeed((p) => ({ ...p, skillName: e.target.value }))}
                className="text-right"
              />
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label>المستوى المطلوب</Label>
                <Select
                  value={String(newNeed.requiredLevel)}
                  onValueChange={(v) => setNewNeed((p) => ({ ...p, requiredLevel: Number(v) }))}
                >
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    {[1, 2, 3, 4, 5].map((l) => (
                      <SelectItem key={l} value={String(l)}>
                        {l}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
              <div className="space-y-2">
                <Label>المستوى الحالي</Label>
                <Select
                  value={String(newNeed.currentLevel)}
                  onValueChange={(v) => setNewNeed((p) => ({ ...p, currentLevel: Number(v) }))}
                >
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    {[1, 2, 3, 4, 5].map((l) => (
                      <SelectItem key={l} value={String(l)}>
                        {l}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setIsAddOpen(false)}>
              إلغاء
            </Button>
            <Button onClick={addNeed}>إضافة</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
