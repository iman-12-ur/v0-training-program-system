import { Card, CardContent } from '@/components/ui/card';
import {
  BookOpen,
  Users,
  ClipboardCheck,
  Clock,
  GraduationCap,
  TrendingUp,
} from 'lucide-react';
import type { DashboardStats } from '@/lib/types';

interface StatsCardsProps {
  stats: DashboardStats;
}

export function StatsCards({ stats }: StatsCardsProps) {
  const cards = [
    {
      title: 'البرامج النشطة',
      value: stats.activePrograms,
      total: stats.totalPrograms,
      icon: <BookOpen className="h-5 w-5" />,
      color: 'bg-blue-500/10 text-blue-600',
      iconBg: 'bg-blue-500',
    },
    {
      title: 'إجمالي التسجيلات',
      value: stats.totalRegistrations,
      icon: <Users className="h-5 w-5" />,
      color: 'bg-emerald-500/10 text-emerald-600',
      iconBg: 'bg-emerald-500',
    },
    {
      title: 'بانتظار الموافقة',
      value: stats.pendingApprovals,
      icon: <Clock className="h-5 w-5" />,
      color: 'bg-amber-500/10 text-amber-600',
      iconBg: 'bg-amber-500',
    },
    {
      title: 'تدريبات مكتملة',
      value: stats.completedTrainings,
      icon: <GraduationCap className="h-5 w-5" />,
      color: 'bg-violet-500/10 text-violet-600',
      iconBg: 'bg-violet-500',
    },
  ];

  return (
    <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
      {cards.map((card, index) => (
        <Card key={index} className="overflow-hidden border-none shadow-sm">
          <CardContent className="p-6">
            <div className="flex items-start justify-between">
              <div className="space-y-2">
                <p className="text-sm font-medium text-muted-foreground">
                  {card.title}
                </p>
                <div className="flex items-baseline gap-2">
                  <p className="text-3xl font-bold text-foreground">{card.value}</p>
                  {card.total && (
                    <span className="text-sm text-muted-foreground">
                      / {card.total}
                    </span>
                  )}
                </div>
              </div>
              <div className={`rounded-xl p-3 ${card.iconBg}`}>
                <div className="text-white">{card.icon}</div>
              </div>
            </div>
          </CardContent>
        </Card>
      ))}
    </div>
  );
}
