import { Card } from "@/components/ui/card"
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table"
import { cn } from "@/lib/utils"
import type { Skill, TrainingHistoryEntry } from "@/lib/training-needs/types"
import { historyInsight } from "@/lib/training-needs/logic"
import { Section } from "./section"

interface PreviousLearningProps {
  history: TrainingHistoryEntry[]
  skills: Skill[]
}

const statusLabels: Record<TrainingHistoryEntry["status"], string> = {
  completed: "مكتمل",
  "in-progress": "قيد التنفيذ",
  "not-started": "لم يبدأ",
}

const resultLabels: Record<TrainingHistoryEntry["result"], string> = {
  improved: "تحسّن واضح",
  partial: "تحسن جزئي",
  "no-change": "دون تغيير",
}

const appliedLabels: Record<TrainingHistoryEntry["applied"], string> = {
  applied: "تم التطبيق",
  partial: "تطبيق جزئي",
  "not-applied": "لم يتم التطبيق",
}

const toneStyles = {
  success: "bg-success/10 text-success ring-success/20",
  warning: "bg-warning/10 text-warning ring-warning/20",
  danger: "bg-destructive/10 text-destructive ring-destructive/20",
} as const

export function PreviousLearning({ history, skills }: PreviousLearningProps) {
  const skillName = (id: string) => skills.find((s) => s.id === id)?.name ?? id

  return (
    <Section
      id="section-history"
      eyebrow="القسم 10"
      title="سجل التعلم السابق"
      description="نستخدم سجلك السابق لتجنّب تكرار تدخل لم يغلق الفجوة دون تحليل السبب."
    >
      <Card className="hidden overflow-hidden p-0 lg:block">
        <Table>
          <TableHeader>
            <TableRow className="bg-muted/50">
              <TableHead className="text-right">اسم البرنامج</TableHead>
              <TableHead className="text-right">التاريخ</TableHead>
              <TableHead className="text-right">المهارة</TableHead>
              <TableHead className="text-right">الحالة</TableHead>
              <TableHead className="text-right">نتيجة التقييم</TableHead>
              <TableHead className="text-right">التطبيق</TableHead>
              <TableHead className="text-right">قراءة المنظومة</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {history.map((entry) => {
              const insight = historyInsight(entry)
              return (
                <TableRow key={entry.id}>
                  <TableCell className="font-medium text-foreground">{entry.program}</TableCell>
                  <TableCell className="text-muted-foreground">{entry.date}</TableCell>
                  <TableCell className="text-muted-foreground">{skillName(entry.skillId)}</TableCell>
                  <TableCell>{statusLabels[entry.status]}</TableCell>
                  <TableCell>{resultLabels[entry.result]}</TableCell>
                  <TableCell>{appliedLabels[entry.applied]}</TableCell>
                  <TableCell>
                    <span
                      className={cn(
                        "inline-flex rounded-full px-2.5 py-0.5 text-xs font-medium ring-1 ring-inset",
                        toneStyles[insight.tone],
                      )}
                    >
                      {insight.text}
                    </span>
                  </TableCell>
                </TableRow>
              )
            })}
          </TableBody>
        </Table>
      </Card>

      <div className="grid gap-4 lg:hidden">
        {history.map((entry) => {
          const insight = historyInsight(entry)
          return (
            <Card key={entry.id} className="space-y-3 p-5">
              <div className="flex items-start justify-between gap-3">
                <h3 className="font-semibold text-foreground">{entry.program}</h3>
                <span className="shrink-0 text-xs text-muted-foreground">{entry.date}</span>
              </div>
              <dl className="grid grid-cols-2 gap-2 text-sm">
                <Field label="المهارة" value={skillName(entry.skillId)} />
                <Field label="الحالة" value={statusLabels[entry.status]} />
                <Field label="النتيجة" value={resultLabels[entry.result]} />
                <Field label="التطبيق" value={appliedLabels[entry.applied]} />
              </dl>
              <p
                className={cn(
                  "rounded-lg px-3 py-2 text-xs font-medium ring-1 ring-inset",
                  toneStyles[insight.tone],
                )}
              >
                {insight.text}
              </p>
            </Card>
          )
        })}
      </div>
    </Section>
  )
}

function Field({ label, value }: { label: string; value: string }) {
  return (
    <div>
      <dt className="text-xs text-muted-foreground">{label}</dt>
      <dd className="font-medium text-foreground">{value}</dd>
    </div>
  )
}
