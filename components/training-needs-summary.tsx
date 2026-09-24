'use client';

import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { ClipboardList, AlertTriangle, ArrowLeft, Lightbulb } from 'lucide-react';
import {
  initialTrainingNeeds,
  getGap,
  getPriority,
  priorityMeta,
  suggestProgram,
  computeReadiness,
} from '@/lib/training-needs';

interface TrainingNeedsSummaryProps {
  onOpen: () => void;
}

export function TrainingNeedsSummary({ onOpen }: TrainingNeedsSummaryProps) {
  const needs = initialTrainingNeeds;
  const highPriority = needs.filter((n) => getPriority(getGap(n)) === 'high').length;
  const readiness = computeReadiness(needs);

  // أعلى 3 فجوات أولوية لعرضها في الملخّص
  const topGaps = [...needs]
    .sort((a, b) => getGap(b) - getGap(a))
    .slice(0, 3);

  return (
    <Card className="border-none shadow-sm">
      <CardHeader className="flex flex-row items-center justify-between gap-4">
        <CardTitle className="flex items-center gap-2 text-lg">
          <ClipboardList className="h-5 w-5 text-primary" />
          الاحتياجات التدريبية
        </CardTitle>
        <Button variant="ghost" size="sm" onClick={onOpen} className="gap-1">
          عرض الكل
          <ArrowLeft className="h-4 w-4" />
        </Button>
      </CardHeader>
      <CardContent className="space-y-4">
        <div className="grid grid-cols-3 gap-3">
          <div className="rounded-lg bg-muted/50 p-3 text-center">
            <p className="text-2xl font-bold text-foreground">{needs.length}</p>
            <p className="text-xs text-muted-foreground">إجمالي الاحتياجات</p>
          </div>
          <div className="rounded-lg bg-red-500/10 p-3 text-center">
            <p className="flex items-center justify-center gap-1 text-2xl font-bold text-red-600">
              <AlertTriangle className="h-5 w-5" />
              {highPriority}
            </p>
            <p className="text-xs text-muted-foreground">أولوية عالية</p>
          </div>
          <div className="rounded-lg bg-emerald-500/10 p-3 text-center">
            <p className="text-2xl font-bold text-emerald-600">{readiness}%</p>
            <p className="text-xs text-muted-foreground">الجاهزية</p>
          </div>
        </div>

        <div className="space-y-2">
          <p className="text-sm font-medium text-muted-foreground">أعلى الفجوات أولوية</p>
          {topGaps.map((need) => {
            const gap = getGap(need);
            const meta = priorityMeta[getPriority(gap)];
            const suggestion = suggestProgram(need);
            return (
              <div key={need.id} className="flex items-center justify-between gap-3 rounded-lg border p-3">
                <div className="min-w-0">
                  <div className="flex items-center gap-2">
                    <span className={`h-2 w-2 shrink-0 rounded-full ${meta.dot}`} />
                    <span className="truncate font-medium text-foreground">{need.skillName}</span>
                  </div>
                  <p className="mt-1 flex items-center gap-1 truncate text-xs text-muted-foreground">
                    <Lightbulb className="h-3 w-3 shrink-0 text-primary" />
                    {suggestion.title}
                  </p>
                </div>
                <div className="flex shrink-0 items-center gap-2">
                  <div className="text-center">
                    <p className="text-xs text-muted-foreground">فجوة</p>
                    <p className="font-bold text-foreground">{gap}</p>
                  </div>
                  <Badge variant="outline" className={meta.className}>{meta.label}</Badge>
                </div>
              </div>
            );
          })}
        </div>
      </CardContent>
    </Card>
  );
}
