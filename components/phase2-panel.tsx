'use client';

import { useMemo, useState } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import {
  Wallet,
  TrendingUp,
  Target,
  GraduationCap,
  AlertTriangle,
  CheckCircle2,
  Clock,
  Info,
} from 'lucide-react';

// موازنة مركزية واحدة لدائرة التدريب — متابعة فقط بلا إيقاف اعتمادات
const centralBudget = {
  financialYear: '2025/2026',
  allocated: 1_200_000,
  committed: 780_000,
  actual: 340_000,
};

// مؤشرات الأداء (KPIs)
const kpis = {
  totalNeeds: 128,
  approvedNeeds: 96,
  approvalRate: 75,
  avgImprovement: 32,
  completedTrainings: 54,
};

// نتائج تقييم الأثر حسب النوع
const impactResults = [
  { type: 'تقييم قبلي/بعدي', completed: 22, pending: 6, avgScore: 4.2, avgImprovement: 38 },
  { type: 'تقييم المدير المباشر', completed: 18, pending: 4, avgScore: 3.9, avgImprovement: 27 },
  { type: 'تقييم ذاتي', completed: 14, pending: 8, avgScore: 4.0, avgImprovement: 31 },
];

const currency = (n: number) => n.toLocaleString('ar-EG');

export function Phase2Panel() {
  const [year] = useState(centralBudget.financialYear);

  const budget = useMemo(() => {
    const consumed = centralBudget.committed + centralBudget.actual;
    const remaining = centralBudget.allocated - consumed;
    const utilization =
      centralBudget.allocated > 0
        ? Math.round((consumed / centralBudget.allocated) * 100)
        : 0;
    return { remaining, utilization, over: remaining < 0 };
  }, []);

  const barColor =
    budget.utilization >= 100
      ? 'bg-red-500'
      : budget.utilization >= 80
        ? 'bg-amber-500'
        : 'bg-emerald-500';

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-2xl font-bold text-foreground">الموازنة والأثر ومؤشرات الأداء</h2>
        <p className="text-muted-foreground">
          موازنة مركزية واحدة لدائرة التدريب مع متابعة الالتزام والمصروف، ونتائج تقييم أثر التدريب
        </p>
      </div>

      {/* تنويه: الموازنة للمتابعة فقط */}
      <div className="flex items-start gap-3 rounded-lg border border-blue-200 bg-blue-500/10 px-4 py-3 text-sm text-blue-700">
        <Info className="mt-0.5 h-4 w-4 shrink-0" />
        <p className="leading-relaxed">
          الموازنة مركزية لدائرة التدريب ومنفصلة عن موازنة الموارد البشرية — تُدار للمتابعة فقط ولا
          توقف اعتماد الاحتياجات التدريبية.
        </p>
      </div>

      {/* الموازنة المركزية */}
      <Card className="border-none shadow-sm">
        <CardHeader className="flex flex-row items-center justify-between">
          <CardTitle className="flex items-center gap-2 text-lg">
            <Wallet className="h-5 w-5 text-primary" />
            الموازنة المركزية لدائرة التدريب
          </CardTitle>
          <Badge variant="outline">السنة المالية {year}</Badge>
        </CardHeader>
        <CardContent className="space-y-5">
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
            <BudgetStat label="المخصص" value={currency(centralBudget.allocated)} className="text-foreground" />
            <BudgetStat label="الملتزم به" value={currency(centralBudget.committed)} className="text-amber-600" />
            <BudgetStat label="المصروف الفعلي" value={currency(centralBudget.actual)} className="text-blue-600" />
            <BudgetStat
              label="المتبقي"
              value={currency(budget.remaining)}
              className={budget.over ? 'text-red-600' : 'text-emerald-600'}
            />
          </div>

          <div>
            <div className="mb-2 flex items-center justify-between text-sm">
              <span className="font-medium text-foreground">نسبة الاستهلاك</span>
              <span className="font-bold text-foreground">{budget.utilization}%</span>
            </div>
            <div className="h-3 w-full overflow-hidden rounded-full bg-muted">
              <div
                className={'h-full rounded-full transition-all ' + barColor}
                style={{ width: `${Math.min(budget.utilization, 100)}%` }}
              />
            </div>
          </div>

          {budget.over && (
            <div className="flex items-center gap-2 rounded-lg border border-amber-200 bg-amber-500/10 px-4 py-3 text-sm text-amber-700">
              <AlertTriangle className="h-4 w-4 shrink-0" />
              تجاوز الاستهلاك المبلغ المخصّص — للمتابعة فقط دون إيقاف أي اعتماد.
            </div>
          )}
        </CardContent>
      </Card>

      {/* مؤشرات الأداء */}
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <KpiCard
          icon={<Target className="h-6 w-6 text-blue-600" />}
          bg="bg-blue-500/10"
          value={`${kpis.approvalRate}%`}
          label="نسبة اعتماد الاحتياجات"
          sub={`${kpis.approvedNeeds} من ${kpis.totalNeeds}`}
        />
        <KpiCard
          icon={<TrendingUp className="h-6 w-6 text-emerald-600" />}
          bg="bg-emerald-500/10"
          value={`${kpis.avgImprovement}%`}
          label="متوسط تحسّن الأداء"
          sub="من تقييمات الأثر المكتملة"
        />
        <KpiCard
          icon={<GraduationCap className="h-6 w-6 text-violet-600" />}
          bg="bg-violet-500/10"
          value={kpis.completedTrainings}
          label="تدريبات منفّذة"
          sub="خلال السنة المالية"
        />
        <KpiCard
          icon={<Wallet className="h-6 w-6 text-amber-600" />}
          bg="bg-amber-500/10"
          value={`${budget.utilization}%`}
          label="استهلاك الموازنة"
          sub={`متبقٍّ ${currency(budget.remaining)}`}
        />
      </div>

      {/* نتائج تقييم الأثر */}
      <Card className="border-none shadow-sm">
        <CardHeader>
          <CardTitle className="flex items-center gap-2 text-lg">
            <TrendingUp className="h-5 w-5 text-primary" />
            نتائج تقييم أثر التدريب
          </CardTitle>
        </CardHeader>
        <CardContent className="overflow-x-auto p-0">
          <table className="w-full text-right text-sm">
            <thead>
              <tr className="border-b text-muted-foreground">
                <th className="p-3 font-medium">نوع التقييم</th>
                <th className="p-3 font-medium">مكتمل</th>
                <th className="p-3 font-medium">قيد الانتظار</th>
                <th className="p-3 font-medium">متوسط الدرجة (من 5)</th>
                <th className="p-3 font-medium">متوسط التحسّن</th>
              </tr>
            </thead>
            <tbody>
              {impactResults.map((r) => (
                <tr key={r.type} className="border-b last:border-0 hover:bg-muted/40">
                  <td className="p-3 font-medium text-foreground">{r.type}</td>
                  <td className="p-3">
                    <span className="inline-flex items-center gap-1 text-emerald-600">
                      <CheckCircle2 className="h-4 w-4" />
                      {r.completed}
                    </span>
                  </td>
                  <td className="p-3">
                    <span className="inline-flex items-center gap-1 text-amber-600">
                      <Clock className="h-4 w-4" />
                      {r.pending}
                    </span>
                  </td>
                  <td className="p-3 font-semibold text-foreground">{r.avgScore}</td>
                  <td className="p-3">
                    <Badge variant="outline" className="border-emerald-200 bg-emerald-500/10 text-emerald-700">
                      +{r.avgImprovement}%
                    </Badge>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </CardContent>
      </Card>
    </div>
  );
}

function BudgetStat({ label, value, className }: { label: string; value: string; className?: string }) {
  return (
    <div className="rounded-lg bg-muted/40 p-4">
      <p className="mb-1 text-xs text-muted-foreground">{label}</p>
      <p className={'text-xl font-bold ' + (className ?? 'text-foreground')}>{value}</p>
    </div>
  );
}

function KpiCard({
  icon,
  bg,
  value,
  label,
  sub,
}: {
  icon: React.ReactNode;
  bg: string;
  value: React.ReactNode;
  label: string;
  sub?: string;
}) {
  return (
    <Card className="border-none shadow-sm">
      <CardContent className="flex items-center gap-4 p-6">
        <div className={'flex h-12 w-12 shrink-0 items-center justify-center rounded-xl ' + bg}>{icon}</div>
        <div>
          <p className="text-2xl font-bold text-foreground">{value}</p>
          <p className="text-sm text-muted-foreground">{label}</p>
          {sub && <p className="text-xs text-muted-foreground/70">{sub}</p>}
        </div>
      </CardContent>
    </Card>
  );
}
