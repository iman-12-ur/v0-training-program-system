'use client';

import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  ResponsiveContainer,
  PieChart,
  Pie,
  Cell,
  Tooltip,
  Legend,
} from 'recharts';
import {
  TrendingUp,
  Users,
  BookOpen,
  Calendar,
  Award,
} from 'lucide-react';
import type { TrainingProgram, Registration, DashboardStats } from '@/lib/types';

interface ReportsViewProps {
  programs: TrainingProgram[];
  registrations: Registration[];
  stats: DashboardStats;
}

export function ReportsView({ programs, registrations, stats }: ReportsViewProps) {
  // Calculate data for charts
  const categoryData = programs.reduce((acc, program) => {
    const existing = acc.find((item) => item.name === program.category);
    if (existing) {
      existing.count++;
    } else {
      acc.push({ name: program.category, count: 1 });
    }
    return acc;
  }, [] as { name: string; count: number }[]);

  const statusData = [
    {
      name: 'بانتظار الموافقة',
      value: registrations.filter((r) => r.status === 'pending').length,
    },
    {
      name: 'موافق عليه',
      value: registrations.filter((r) => r.status === 'approved').length,
    },
    {
      name: 'مكتمل',
      value: registrations.filter((r) => r.status === 'completed').length,
    },
    {
      name: 'مرفوض',
      value: registrations.filter((r) => r.status === 'rejected').length,
    },
  ].filter((item) => item.value > 0);

  const programRegistrations = programs
    .filter((p) => p.status === 'active')
    .map((program) => ({
      name: program.title.length > 15 ? program.title.slice(0, 15) + '...' : program.title,
      registrations: registrations.filter((r) => r.programId === program.id).length,
      capacity: program.batches.reduce((sum, b) => sum + b.maxParticipants, 0),
    }))
    .slice(0, 5);

  const COLORS = ['#3b82f6', '#10b981', '#8b5cf6', '#f59e0b', '#ef4444'];

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h2 className="text-2xl font-bold text-foreground">التقارير والإحصائيات</h2>
        <p className="text-muted-foreground">
          نظرة شاملة على أداء البرامج التدريبية
        </p>
      </div>

      {/* Summary Cards */}
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <Card className="border-none shadow-sm">
          <CardContent className="flex items-center gap-4 p-6">
            <div className="rounded-full bg-blue-500/10 p-3">
              <BookOpen className="h-6 w-6 text-blue-600" />
            </div>
            <div>
              <p className="text-sm text-muted-foreground">إجمالي البرامج</p>
              <p className="text-2xl font-bold">{stats.totalPrograms}</p>
            </div>
          </CardContent>
        </Card>

        <Card className="border-none shadow-sm">
          <CardContent className="flex items-center gap-4 p-6">
            <div className="rounded-full bg-emerald-500/10 p-3">
              <Users className="h-6 w-6 text-emerald-600" />
            </div>
            <div>
              <p className="text-sm text-muted-foreground">إجمالي التسجيلات</p>
              <p className="text-2xl font-bold">{stats.totalRegistrations}</p>
            </div>
          </CardContent>
        </Card>

        <Card className="border-none shadow-sm">
          <CardContent className="flex items-center gap-4 p-6">
            <div className="rounded-full bg-violet-500/10 p-3">
              <Award className="h-6 w-6 text-violet-600" />
            </div>
            <div>
              <p className="text-sm text-muted-foreground">تدريبات مكتملة</p>
              <p className="text-2xl font-bold">{stats.completedTrainings}</p>
            </div>
          </CardContent>
        </Card>

        <Card className="border-none shadow-sm">
          <CardContent className="flex items-center gap-4 p-6">
            <div className="rounded-full bg-amber-500/10 p-3">
              <TrendingUp className="h-6 w-6 text-amber-600" />
            </div>
            <div>
              <p className="text-sm text-muted-foreground">نسبة الإكمال</p>
              <p className="text-2xl font-bold">
                {stats.totalRegistrations > 0
                  ? Math.round(
                      (stats.completedTrainings / stats.totalRegistrations) * 100
                    )
                  : 0}
                %
              </p>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Charts */}
      <div className="grid gap-6 lg:grid-cols-2">
        {/* Registrations by Program */}
        <Card className="border-none shadow-sm">
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Calendar className="h-5 w-5 text-primary" />
              التسجيلات حسب البرنامج
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="h-64">
              <ResponsiveContainer width="100%" height="100%">
                <BarChart data={programRegistrations} layout="vertical">
                  <CartesianGrid strokeDasharray="3 3" horizontal={false} />
                  <XAxis type="number" />
                  <YAxis
                    dataKey="name"
                    type="category"
                    width={100}
                    tick={{ fontSize: 12 }}
                  />
                  <Tooltip
                    contentStyle={{
                      backgroundColor: 'hsl(var(--card))',
                      border: '1px solid hsl(var(--border))',
                      borderRadius: '8px',
                    }}
                  />
                  <Bar
                    dataKey="registrations"
                    fill="#3b82f6"
                    radius={[0, 4, 4, 0]}
                    name="التسجيلات"
                  />
                </BarChart>
              </ResponsiveContainer>
            </div>
          </CardContent>
        </Card>

        {/* Registration Status Distribution */}
        <Card className="border-none shadow-sm">
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Users className="h-5 w-5 text-primary" />
              توزيع حالات التسجيل
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="h-64">
              <ResponsiveContainer width="100%" height="100%">
                <PieChart>
                  <Pie
                    data={statusData}
                    cx="50%"
                    cy="50%"
                    innerRadius={60}
                    outerRadius={80}
                    paddingAngle={5}
                    dataKey="value"
                    label={({ name, percent }) =>
                      `${name} (${(percent * 100).toFixed(0)}%)`
                    }
                    labelLine={false}
                  >
                    {statusData.map((entry, index) => (
                      <Cell
                        key={`cell-${index}`}
                        fill={COLORS[index % COLORS.length]}
                      />
                    ))}
                  </Pie>
                  <Tooltip
                    contentStyle={{
                      backgroundColor: 'hsl(var(--card))',
                      border: '1px solid hsl(var(--border))',
                      borderRadius: '8px',
                    }}
                  />
                </PieChart>
              </ResponsiveContainer>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Programs by Category */}
      <Card className="border-none shadow-sm">
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <BookOpen className="h-5 w-5 text-primary" />
            البرامج حسب التصنيف
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="flex flex-wrap gap-3">
            {categoryData.map((cat, index) => (
              <Badge
                key={cat.name}
                variant="secondary"
                className="px-4 py-2 text-sm"
                style={{
                  backgroundColor: `${COLORS[index % COLORS.length]}20`,
                  color: COLORS[index % COLORS.length],
                }}
              >
                {cat.name}: {cat.count} برنامج
              </Badge>
            ))}
          </div>
        </CardContent>
      </Card>

      {/* Top Performing Programs */}
      <Card className="border-none shadow-sm">
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <TrendingUp className="h-5 w-5 text-primary" />
            البرامج الأكثر طلباً
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="space-y-4">
            {programRegistrations
              .sort((a, b) => b.registrations - a.registrations)
              .slice(0, 5)
              .map((program, index) => (
                <div
                  key={program.name}
                  className="flex items-center justify-between"
                >
                  <div className="flex items-center gap-3">
                    <div
                      className="flex h-8 w-8 items-center justify-center rounded-full text-sm font-bold text-white"
                      style={{ backgroundColor: COLORS[index % COLORS.length] }}
                    >
                      {index + 1}
                    </div>
                    <span className="font-medium">{program.name}</span>
                  </div>
                  <div className="flex items-center gap-2">
                    <span className="text-sm text-muted-foreground">
                      {program.registrations} تسجيل
                    </span>
                    <div className="h-2 w-24 overflow-hidden rounded-full bg-muted">
                      <div
                        className="h-full rounded-full"
                        style={{
                          width: `${
                            program.capacity > 0
                              ? (program.registrations / program.capacity) * 100
                              : 0
                          }%`,
                          backgroundColor: COLORS[index % COLORS.length],
                        }}
                      />
                    </div>
                  </div>
                </div>
              ))}
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
