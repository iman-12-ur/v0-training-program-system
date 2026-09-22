"use client"

import { Card } from "@/components/ui/card"
import { Label } from "@/components/ui/label"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { cn } from "@/lib/utils"
import type { ChallengeLevel, JobTask, ObstacleType } from "@/lib/training-needs/types"
import { Section } from "./section"

interface JobTasksProps {
  tasks: JobTask[]
  onChallengeChange: (taskId: string, challenge: ChallengeLevel) => void
  hardestTaskId: string
  onHardestTaskChange: (taskId: string) => void
  obstacle: ObstacleType | ""
  onObstacleChange: (obstacle: ObstacleType) => void
}

const challengeOptions: { value: ChallengeLevel; label: string }[] = [
  { value: "none", label: "لا يوجد" },
  { value: "low", label: "بسيط" },
  { value: "medium", label: "متوسط" },
  { value: "high", label: "مرتفع" },
]

const challengeStyles: Record<ChallengeLevel, string> = {
  none: "data-[state=on]:bg-success data-[state=on]:text-success-foreground",
  low: "data-[state=on]:bg-info data-[state=on]:text-info-foreground",
  medium: "data-[state=on]:bg-warning data-[state=on]:text-warning-foreground",
  high: "data-[state=on]:bg-destructive data-[state=on]:text-destructive-foreground",
}

const obstacleOptions: { value: ObstacleType; label: string }[] = [
  { value: "knowledge", label: "معرفة" },
  { value: "skill", label: "مهارة" },
  { value: "practice", label: "ممارسة" },
  { value: "system", label: "نظام / أداة" },
  { value: "resources", label: "موارد" },
  { value: "unknown", label: "غير متأكد" },
]

function ChallengeSelector({
  taskId,
  value,
  onChange,
}: {
  taskId: string
  value: ChallengeLevel
  onChange: (v: ChallengeLevel) => void
}) {
  return (
    <div role="radiogroup" aria-label="مستوى التحدي" className="flex flex-wrap gap-2">
      {challengeOptions.map((opt) => {
        const active = value === opt.value
        return (
          <button
            key={opt.value}
            type="button"
            role="radio"
            aria-checked={active}
            data-state={active ? "on" : "off"}
            onClick={() => onChange(opt.value)}
            className={cn(
              "rounded-lg border border-border px-3 py-1.5 text-xs font-medium text-muted-foreground transition-colors hover:border-primary/40 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring",
              challengeStyles[opt.value],
            )}
          >
            {opt.label}
          </button>
        )
      })}
    </div>
  )
}

export function JobTasks({
  tasks,
  onChallengeChange,
  hardestTaskId,
  onHardestTaskChange,
  obstacle,
  onObstacleChange,
}: JobTasksProps) {
  return (
    <Section
      id="section-tasks"
      eyebrow="القسم 01"
      title="مهامي الرئيسية"
      description="حدد مستوى التحدي في كل مهمة لنربط الفجوات بالمهام الفعلية لوظيفتك."
    >
      <div className="grid gap-4">
        {tasks.map((task) => (
          <Card key={task.id} className="p-5">
            <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
              <div className="min-w-0">
                <h3 className="text-base font-semibold text-foreground">{task.name}</h3>
                <p className="mt-1 text-sm text-muted-foreground">{task.description}</p>
              </div>
              <div className="shrink-0">
                <span className="mb-1.5 block text-xs font-medium text-muted-foreground">مستوى التحدي</span>
                <ChallengeSelector
                  taskId={task.id}
                  value={task.challenge}
                  onChange={(v) => onChallengeChange(task.id, v)}
                />
              </div>
            </div>
          </Card>
        ))}
      </div>

      <div className="mt-6 grid gap-5 rounded-xl border border-border bg-muted/30 p-5 sm:grid-cols-2">
        <div className="space-y-2">
          <Label htmlFor="hardest-task" className="text-sm font-medium">
            ما أكثر مهمة تواجه فيها تحدياً حالياً؟
          </Label>
          <Select value={hardestTaskId} onValueChange={onHardestTaskChange}>
            <SelectTrigger id="hardest-task">
              <SelectValue placeholder="اختر المهمة" />
            </SelectTrigger>
            <SelectContent>
              {tasks.map((task) => (
                <SelectItem key={task.id} value={task.id}>
                  {task.name}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>
        <div className="space-y-2">
          <Label htmlFor="main-obstacle" className="text-sm font-medium">
            ما العائق الرئيسي؟
          </Label>
          <Select value={obstacle} onValueChange={(v) => onObstacleChange(v as ObstacleType)}>
            <SelectTrigger id="main-obstacle">
              <SelectValue placeholder="اختر العائق" />
            </SelectTrigger>
            <SelectContent>
              {obstacleOptions.map((opt) => (
                <SelectItem key={opt.value} value={opt.value}>
                  {opt.label}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>
      </div>
    </Section>
  )
}
