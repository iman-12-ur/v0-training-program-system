'use client';

import { useMemo, useRef, useState } from 'react';
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
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  Cell,
} from 'recharts';
import {
  ClipboardPlus,
  Plus,
  Search,
  AlertTriangle,
  Gauge,
  Trash2,
  TrendingUp,
  Upload,
  Download,
  Lightbulb,
  Users,
  LayoutGrid,
  TableIcon,
  Loader2,
} from 'lucide-react';
import {
  type TrainingNeed,
  type NeedStatus,
  initialTrainingNeeds,
  trainingNeedDepartments as departments,
  trainingNeedCategories as categories,
  getGap,
  getPriority,
  priorityMeta,
  suggestProgram,
} from '@/lib/training-needs';

export function TrainingNeedsView() {
  const [needs, setNeeds] = useState<TrainingNeed[]>(initialTrainingNeeds);
  const [departmentFilter, setDepartmentFilter] = useState('الكل');
  const [priorityFilter, setPriorityFilter] = useState('الكل');
  const [statusFilter, setStatusFilter] = useState('الكل');
  const [search, setSearch] = useState('');
  const [view, setView] = useState<'grouped' | 'table'>('grouped');
  const [isAddOpen, setIsAddOpen] = useState(false);
  const fileRef = useRef<HTMLInputElement>(null);
  const [importMsg, setImportMsg] = useState<string | null>(null);
  const [newNeed, setNewNeed] = useState({
    employeeName: '',
    employeeNumber: '',
    department: 'تقنية المعلومات',
    skillName: '',
    category: 'تقنية المعلومات',
    requiredLevel: 4,
    currentLevel: 2,
  });

  const filteredNeeds = useMemo(() => {
    return needs.filter((n) => {
      if (departmentFilter !== 'الكل' && n.department !== departmentFilter) return false;
      if (priorityFilter !== 'الكل' && getPriority(getGap(n)) !== priorityFilter) return false;
      if (statusFilter !== 'الكل' && n.status !== statusFilter) return false;
      if (search.trim()) {
        const q = search.trim();
        if (
          !n.employeeName.includes(q) &&
          !n.employeeNumber.includes(q) &&
          !n.skillName.includes(q)
        )
          return false;
      }
      return true;
    });
  }, [needs, departmentFilter, priorityFilter, statusFilter, search]);

  const stats = useMemo(() => {
    const total = filteredNeeds.length;
    const highPriority = filteredNeeds.filter((n) => getPriority(getGap(n)) === 'high').length;
    const inProgress = filteredNeeds.filter((n) => n.status === 'in_progress').length;
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
    return { total, highPriority, inProgress, readiness };
  }, [filteredNeeds]);

  // متوسط الفجوة لكل دائرة (للرسم البياني)
  const gapByDept = useMemo(() => {
    const map = new Map<string, { sum: number; count: number }>();
    filteredNeeds.forEach((n) => {
      const cur = map.get(n.department) ?? { sum: 0, count: 0 };
      cur.sum += getGap(n);
      cur.count += 1;
      map.set(n.department, cur);
    });
    return Array.from(map.entries()).map(([name, v]) => ({
      name,
      gap: Number((v.sum / v.count).toFixed(1)),
    }));
  }, [filteredNeeds]);

  // تجميع حسب الموظف
  const grouped = useMemo(() => {
    const map = new Map<string, TrainingNeed[]>();
    filteredNeeds.forEach((n) => {
      const key = `${n.employeeName}||${n.employeeNumber}||${n.department}`;
      const arr = map.get(key) ?? [];
      arr.push(n);
      map.set(key, arr);
    });
    return Array.from(map.entries()).map(([key, items]) => {
      const [employeeName, employeeNumber, department] = key.split('||');
      const avgReadiness = Math.round(
        (items.reduce((s, n) => s + Math.min(n.currentLevel, n.requiredLevel) / n.requiredLevel, 0) /
          items.length) *
          100
      );
      return { employeeName, employeeNumber, department, items, avgReadiness };
    });
  }, [filteredNeeds]);

  const updateCurrentLevel = (id: string, level: number) =>
    setNeeds((prev) => prev.map((n) => (n.id === id ? { ...n, currentLevel: level } : n)));

  const updateStatus = (id: string, status: NeedStatus) =>
    setNeeds((prev) => prev.map((n) => (n.id === id ? { ...n, status } : n)));

  const deleteNeed = (id: string) => setNeeds((prev) => prev.filter((n) => n.id !== id));

  const addNeed = () => {
    if (!newNeed.employeeName.trim() || !newNeed.skillName.trim()) return;
    setNeeds((prev) => [{ id: `n-${Date.now()}`, status: 'new', ...newNeed }, ...prev]);
    setIsAddOpen(false);
    setNewNeed({
      employeeName: '',
      employeeNumber: '',
      department: 'تقنية المعلومات',
      skillName: '',
      category: 'تقنية المعلومات',
      requiredLevel: 4,
      currentLevel: 2,
    });
  };

  const downloadTemplate = () => {
    const headers = ['الرقم الوظيفي', 'اسم الموظف', 'الدائرة', 'المهارة', 'التصنيف', 'المستوى المطلوب', 'المستوى الحالي'];
    const example = ['EMP-1024', 'أحمد المطيري', 'تقنية المعلومات', 'تحليل البيانات', 'تقنية المعلومات', '5', '2'];
    const csv = '\uFEFF' + [headers.join(','), example.join(',')].join('\n');
    const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = 'قالب_الاحتياجات_التدريبية.csv';
    a.click();
    URL.revokeObjectURL(url);
  };

  const handleImport = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;
    const reader = new FileReader();
    reader.onload = () => {
      const text = String(reader.result || '').replace(/^\uFEFF/, '');
      const lines = text.split(/\r?\n/).filter((l) => l.trim());
      const rows = lines.slice(1); // تجاوز رأس الأعمدة
      const parsed: TrainingNeed[] = [];
      rows.forEach((line, i) => {
        const cols = line.split(',').map((c) => c.trim());
        if (cols.length < 7) return;
        const required = Number(cols[5]) || 0;
        const current = Number(cols[6]) || 0;
        if (!cols[1] || !cols[3]) return;
        parsed.push({
          id: `imp-${Date.now()}-${i}`,
          employeeNumber: cols[0],
          employeeName: cols[1],
          department: cols[2] || 'غير محدد',
          skillName: cols[3],
          category: cols[4] || 'غير محدد',
          requiredLevel: Math.min(5, Math.max(1, required)),
          currentLevel: Math.min(5, Math.max(0, current)),
          status: 'new',
        });
      });
      if (parsed.length) {
        setNeeds((prev) => [...parsed, ...prev]);
        setImportMsg(`تم استيراد ${parsed.length} سجلاً بنجاح`);
      } else {
        setImportMsg('لم يتم العثور على سجلات صالحة في الملف');
      }
      setTimeout(() => setImportMsg(null), 4000);
    };
    reader.readAsText(file, 'utf-8');
    e.target.value = '';
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
        <div>
          <h2 className="text-2xl font-bold text-foreground">الاحتياجات التدريبية</h2>
          <p className="text-muted-foreground">
            تحديد الفجوة المهارية لكل موظف ثم ربطها بالبرنامج التدريبي المناسب لسدّها
          </p>
        </div>
        <div className="flex flex-wrap gap-2">
          <input ref={fileRef} type="file" accept=".csv" hidden onChange={handleImport} />
          <Button variant="outline" onClick={downloadTemplate}>
            <Download className="ml-2 h-4 w-4" />
            تنزيل القالب
          </Button>
          <Button variant="outline" onClick={() => fileRef.current?.click()}>
            <Upload className="ml-2 h-4 w-4" />
            رفع Excel/CSV
          </Button>
          <Button onClick={() => setIsAddOpen(true)}>
            <Plus className="ml-2 h-4 w-4" />
            إضافة احتياج
          </Button>
        </div>
      </div>

      {importMsg && (
        <div className="rounded-lg border border-emerald-200 bg-emerald-500/10 px-4 py-3 text-sm text-emerald-700">
          {importMsg}
        </div>
      )}

      {/* How it works */}
      <Card className="border-none bg-muted/40 shadow-sm">
        <CardContent className="p-6">
          <p className="mb-4 font-semibold text-foreground">كيف تعمل الاحتياجات التدريبية؟</p>
          <div className="grid gap-4 sm:grid-cols-3">
            {[
              { n: '1', t: 'تحديد الفجوة', d: 'قارن المستوى المطلوب للمهارة بالمستوى الحالي للموظف. الفارق = الفجوة.' },
              { n: '2', t: 'ترتيب الأولوية', d: 'يحسب النظام الأولوية تلقائياً: فجوة 3+ عالية، 2 متوسطة، 1 منخفضة.' },
              { n: '3', t: 'ربط ببرنامج', d: 'يقترح النظام برنامجاً تدريبياً من البرامج المتاحة لسدّ كل فجوة.' },
            ].map((step) => (
              <div key={step.n} className="flex gap-3">
                <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-primary text-sm font-bold text-primary-foreground">
                  {step.n}
                </div>
                <div>
                  <p className="font-medium text-foreground">{step.t}</p>
                  <p className="text-sm leading-relaxed text-muted-foreground">{step.d}</p>
                </div>
              </div>
            ))}
          </div>
        </CardContent>
      </Card>

      {/* Stats */}
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <StatCard icon={<ClipboardPlus className="h-6 w-6 text-blue-600" />} bg="bg-blue-500/10" value={stats.total} label="إجمالي الاحتياجات" />
        <StatCard icon={<AlertTriangle className="h-6 w-6 text-red-600" />} bg="bg-red-500/10" value={stats.highPriority} label="فجوات ذات أولوية عالية" />
        <StatCard icon={<Loader2 className="h-6 w-6 text-amber-600" />} bg="bg-amber-500/10" value={stats.inProgress} label="قيد المعالجة" />
        <StatCard icon={<Gauge className="h-6 w-6 text-emerald-600" />} bg="bg-emerald-500/10" value={`${stats.readiness}%`} label="مؤشر الجاهزية" />
      </div>

      {/* Chart */}
      {gapByDept.length > 0 && (
        <Card className="border-none shadow-sm">
          <CardHeader>
            <CardTitle className="flex items-center gap-2 text-lg">
              <TrendingUp className="h-5 w-5 text-primary" />
              متوسط الفجوة حسب الدائرة
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="h-64 w-full" dir="ltr">
              <ResponsiveContainer width="100%" height="100%">
                <BarChart data={gapByDept} margin={{ top: 8, right: 8, left: 8, bottom: 8 }}>
                  <CartesianGrid strokeDasharray="3 3" className="stroke-border" vertical={false} />
                  <XAxis dataKey="name" tick={{ fontSize: 12 }} interval={0} />
                  <YAxis domain={[0, 5]} tick={{ fontSize: 12 }} allowDecimals />
                  <Tooltip
                    formatter={(v: number) => [`${v}`, 'متوسط الفجوة']}
                    contentStyle={{ direction: 'rtl', borderRadius: 8, border: '1px solid hsl(var(--border))' }}
                  />
                  <Bar dataKey="gap" radius={[6, 6, 0, 0]}>
                    {gapByDept.map((entry, i) => {
                      const p = getPriority(Math.round(entry.gap));
                      const color = p === 'high' ? '#ef4444' : p === 'medium' ? '#f59e0b' : '#10b981';
                      return <Cell key={i} fill={color} />;
                    })}
                  </Bar>
                </BarChart>
              </ResponsiveContainer>
            </div>
          </CardContent>
        </Card>
      )}

      {/* Filters */}
      <div className="flex flex-col gap-3 lg:flex-row lg:items-center lg:justify-between">
        <div className="flex flex-1 flex-wrap items-center gap-3">
          <div className="relative flex-1 min-w-[200px]">
            <Search className="absolute right-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
            <Input
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              placeholder="بحث بالاسم أو الرقم الوظيفي أو المهارة"
              className="pr-9 text-right"
            />
          </div>
          <FilterSelect value={departmentFilter} onChange={setDepartmentFilter} options={['الكل', ...departments]} placeholder="الدائرة" />
          <Select value={priorityFilter} onValueChange={setPriorityFilter}>
            <SelectTrigger className="w-36"><SelectValue placeholder="الأولوية" /></SelectTrigger>
            <SelectContent>
              <SelectItem value="الكل">كل الأولويات</SelectItem>
              <SelectItem value="high">عالية</SelectItem>
              <SelectItem value="medium">متوسطة</SelectItem>
              <SelectItem value="low">منخفضة</SelectItem>
            </SelectContent>
          </Select>
          <Select value={statusFilter} onValueChange={setStatusFilter}>
            <SelectTrigger className="w-36"><SelectValue placeholder="الحالة" /></SelectTrigger>
            <SelectContent>
              <SelectItem value="الكل">كل الحالات</SelectItem>
              <SelectItem value="new">جديد</SelectItem>
              <SelectItem value="in_progress">قيد المعالجة</SelectItem>
              <SelectItem value="completed">مكتمل</SelectItem>
            </SelectContent>
          </Select>
        </div>
        <div className="flex items-center gap-1 rounded-lg border p-1">
          <Button variant={view === 'grouped' ? 'secondary' : 'ghost'} size="sm" onClick={() => setView('grouped')}>
            <Users className="ml-1 h-4 w-4" />
            حسب الموظف
          </Button>
          <Button variant={view === 'table' ? 'secondary' : 'ghost'} size="sm" onClick={() => setView('table')}>
            <TableIcon className="ml-1 h-4 w-4" />
            جدول
          </Button>
        </div>
      </div>

      {filteredNeeds.length === 0 ? (
        <Card className="border-none shadow-sm">
          <CardContent className="flex flex-col items-center justify-center py-16 text-center">
            <ClipboardPlus className="h-10 w-10 text-muted-foreground" />
            <p className="mt-3 text-muted-foreground">لا توجد احتياجات مطابقة للتصفية الحالية</p>
          </CardContent>
        </Card>
      ) : view === 'grouped' ? (
        <div className="space-y-4">
          {grouped.map((g) => (
            <Card key={g.employeeNumber + g.employeeName} className="border-none shadow-sm">
              <CardHeader className="flex flex-row items-center justify-between gap-4 pb-3">
                <div className="flex items-center gap-3">
                  <div className="flex h-10 w-10 items-center justify-center rounded-full bg-primary/10 font-bold text-primary">
                    {g.employeeName.charAt(0)}
                  </div>
                  <div>
                    <CardTitle className="text-base">{g.employeeName}</CardTitle>
                    <p className="text-xs text-muted-foreground">{g.employeeNumber} · {g.department}</p>
                  </div>
                </div>
                <div className="text-left">
                  <p className="text-lg font-bold text-foreground">{g.avgReadiness}%</p>
                  <p className="text-xs text-muted-foreground">الجاهزية</p>
                </div>
              </CardHeader>
              <CardContent className="space-y-2">
                {g.items.map((need) => (
                  <NeedRow
                    key={need.id}
                    need={need}
                    onLevel={updateCurrentLevel}
                    onStatus={updateStatus}
                    onDelete={deleteNeed}
                  />
                ))}
              </CardContent>
            </Card>
          ))}
        </div>
      ) : (
        <Card className="border-none shadow-sm">
          <CardContent className="overflow-x-auto p-0">
            <table className="w-full text-right text-sm">
              <thead>
                <tr className="border-b text-muted-foreground">
                  <th className="p-3 font-medium">الموظف</th>
                  <th className="p-3 font-medium">المهارة</th>
                  <th className="p-3 font-medium">المطلوب</th>
                  <th className="p-3 font-medium">الحالي</th>
                  <th className="p-3 font-medium">الفجوة</th>
                  <th className="p-3 font-medium">الأولوية</th>
                  <th className="p-3 font-medium">البرنامج المقترح</th>
                  <th className="p-3 font-medium">الحالة</th>
                  <th className="p-3 font-medium"></th>
                </tr>
              </thead>
              <tbody>
                {filteredNeeds.map((need) => {
                  const gap = getGap(need);
                  const priority = getPriority(gap);
                  const meta = priorityMeta[priority];
                  const suggestion = suggestProgram(need);
                  return (
                    <tr key={need.id} className="border-b last:border-0 hover:bg-muted/40">
                      <td className="p-3">
                        <div className="font-medium text-foreground">{need.employeeName}</div>
                        <div className="text-xs text-muted-foreground">{need.employeeNumber} · {need.department}</div>
                      </td>
                      <td className="p-3">
                        <div className="font-medium text-foreground">{need.skillName}</div>
                        <div className="text-xs text-muted-foreground">{need.category}</div>
                      </td>
                      <td className="p-3 font-semibold text-foreground">{need.requiredLevel}</td>
                      <td className="p-3">
                        <LevelPicker value={need.currentLevel} onChange={(l) => updateCurrentLevel(need.id, l)} />
                      </td>
                      <td className="p-3">
                        <span className="inline-flex h-6 w-6 items-center justify-center rounded-full bg-muted font-semibold text-foreground">
                          {gap}
                        </span>
                      </td>
                      <td className="p-3">
                        <Badge variant="outline" className={meta.className}>{meta.label}</Badge>
                      </td>
                      <td className="p-3">
                        <div className={'flex items-center gap-1 text-xs ' + (suggestion.matched ? 'text-foreground' : 'text-amber-600')}>
                          <Lightbulb className="h-3.5 w-3.5 shrink-0" />
                          <span>{suggestion.title}</span>
                        </div>
                      </td>
                      <td className="p-3">
                        <StatusSelect value={need.status} onChange={(s) => updateStatus(need.id, s)} />
                      </td>
                      <td className="p-3">
                        <Button variant="ghost" size="icon" className="h-8 w-8 text-muted-foreground hover:text-red-600" onClick={() => deleteNeed(need.id)}>
                          <Trash2 className="h-4 w-4" />
                        </Button>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </CardContent>
        </Card>
      )}

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
                <Input value={newNeed.employeeName} onChange={(e) => setNewNeed((p) => ({ ...p, employeeName: e.target.value }))} className="text-right" />
              </div>
              <div className="space-y-2">
                <Label>الرقم الوظيفي</Label>
                <Input value={newNeed.employeeNumber} onChange={(e) => setNewNeed((p) => ({ ...p, employeeNumber: e.target.value }))} className="text-right" />
              </div>
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label>الدائرة</Label>
                <Select value={newNeed.department} onValueChange={(v) => setNewNeed((p) => ({ ...p, department: v }))}>
                  <SelectTrigger><SelectValue /></SelectTrigger>
                  <SelectContent>
                    {departments.map((d) => <SelectItem key={d} value={d}>{d}</SelectItem>)}
                  </SelectContent>
                </Select>
              </div>
              <div className="space-y-2">
                <Label>التصنيف</Label>
                <Select value={newNeed.category} onValueChange={(v) => setNewNeed((p) => ({ ...p, category: v }))}>
                  <SelectTrigger><SelectValue /></SelectTrigger>
                  <SelectContent>
                    {categories.map((c) => <SelectItem key={c} value={c}>{c}</SelectItem>)}
                  </SelectContent>
                </Select>
              </div>
            </div>
            <div className="space-y-2">
              <Label>المهارة</Label>
              <Input value={newNeed.skillName} onChange={(e) => setNewNeed((p) => ({ ...p, skillName: e.target.value }))} className="text-right" />
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label>المستوى المطلوب</Label>
                <Select value={String(newNeed.requiredLevel)} onValueChange={(v) => setNewNeed((p) => ({ ...p, requiredLevel: Number(v) }))}>
                  <SelectTrigger><SelectValue /></SelectTrigger>
                  <SelectContent>
                    {[1, 2, 3, 4, 5].map((l) => <SelectItem key={l} value={String(l)}>{l}</SelectItem>)}
                  </SelectContent>
                </Select>
              </div>
              <div className="space-y-2">
                <Label>المستوى الحالي</Label>
                <Select value={String(newNeed.currentLevel)} onValueChange={(v) => setNewNeed((p) => ({ ...p, currentLevel: Number(v) }))}>
                  <SelectTrigger><SelectValue /></SelectTrigger>
                  <SelectContent>
                    {[1, 2, 3, 4, 5].map((l) => <SelectItem key={l} value={String(l)}>{l}</SelectItem>)}
                  </SelectContent>
                </Select>
              </div>
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setIsAddOpen(false)}>إلغاء</Button>
            <Button onClick={addNeed}>إضافة</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}

function StatCard({ icon, bg, value, label }: { icon: React.ReactNode; bg: string; value: React.ReactNode; label: string }) {
  return (
    <Card className="border-none shadow-sm">
      <CardContent className="flex items-center gap-4 p-6">
        <div className={`rounded-full p-3 ${bg}`}>{icon}</div>
        <div>
          <p className="text-2xl font-bold text-foreground">{value}</p>
          <p className="text-sm text-muted-foreground">{label}</p>
        </div>
      </CardContent>
    </Card>
  );
}

function FilterSelect({ value, onChange, options, placeholder }: { value: string; onChange: (v: string) => void; options: string[]; placeholder: string }) {
  return (
    <Select value={value} onValueChange={onChange}>
      <SelectTrigger className="w-44"><SelectValue placeholder={placeholder} /></SelectTrigger>
      <SelectContent>
        {options.map((o) => <SelectItem key={o} value={o}>{o === 'الكل' ? `كل الدوائر` : o}</SelectItem>)}
      </SelectContent>
    </Select>
  );
}

function LevelPicker({ value, onChange }: { value: number; onChange: (l: number) => void }) {
  return (
    <div className="flex gap-1" dir="rtl">
      {[1, 2, 3, 4, 5].map((lvl) => (
        <button
          key={lvl}
          type="button"
          aria-label={`المستوى ${lvl}`}
          onClick={() => onChange(lvl)}
          className={
            'h-4 w-6 rounded border transition-colors ' +
            (lvl <= value ? 'border-primary bg-primary' : 'border-border bg-muted hover:border-primary')
          }
        />
      ))}
    </div>
  );
}

function StatusSelect({ value, onChange }: { value: NeedStatus; onChange: (s: NeedStatus) => void }) {
  return (
    <Select value={value} onValueChange={(v) => onChange(v as NeedStatus)}>
      <SelectTrigger className="h-8 w-32 text-xs"><SelectValue /></SelectTrigger>
      <SelectContent>
        <SelectItem value="new">جديد</SelectItem>
        <SelectItem value="in_progress">قيد المعالجة</SelectItem>
        <SelectItem value="completed">مكتمل</SelectItem>
      </SelectContent>
    </Select>
  );
}

function NeedRow({
  need,
  onLevel,
  onStatus,
  onDelete,
}: {
  need: TrainingNeed;
  onLevel: (id: string, l: number) => void;
  onStatus: (id: string, s: NeedStatus) => void;
  onDelete: (id: string) => void;
}) {
  const gap = getGap(need);
  const priority = getPriority(gap);
  const meta = priorityMeta[priority];
  const suggestion = suggestProgram(need);
  return (
    <div className="flex flex-col gap-3 rounded-lg border p-3 lg:flex-row lg:items-center">
      <div className="flex-1">
        <div className="flex items-center gap-2">
          <span className={`h-2 w-2 rounded-full ${meta.dot}`} />
          <span className="font-medium text-foreground">{need.skillName}</span>
          <Badge variant="outline" className={meta.className + ' text-[11px]'}>{meta.label}</Badge>
        </div>
        <div className="mt-1 flex items-center gap-1 text-xs text-muted-foreground">
          <Lightbulb className={'h-3.5 w-3.5 ' + (suggestion.matched ? 'text-primary' : 'text-amber-500')} />
          <span>البرنامج المقترح: {suggestion.title}</span>
        </div>
      </div>
      <div className="flex flex-wrap items-center gap-4">
        <div className="text-center">
          <p className="text-xs text-muted-foreground">مطلوب</p>
          <p className="font-semibold text-foreground">{need.requiredLevel}</p>
        </div>
        <div>
          <p className="mb-1 text-xs text-muted-foreground">الحالي</p>
          <LevelPicker value={need.currentLevel} onChange={(l) => onLevel(need.id, l)} />
        </div>
        <div className="text-center">
          <p className="text-xs text-muted-foreground">الفجوة</p>
          <p className="font-semibold text-foreground">{gap}</p>
        </div>
        <StatusSelect value={need.status} onChange={(s) => onStatus(need.id, s)} />
        <Button variant="ghost" size="icon" className="h-8 w-8 text-muted-foreground hover:text-red-600" onClick={() => onDelete(need.id)}>
          <Trash2 className="h-4 w-4" />
        </Button>
      </div>
    </div>
  );
}
