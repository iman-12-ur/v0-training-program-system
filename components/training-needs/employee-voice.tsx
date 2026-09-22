"use client"

import { Card } from "@/components/ui/card"
import { Label } from "@/components/ui/label"
import { Textarea } from "@/components/ui/textarea"
import { cn } from "@/lib/utils"
import type { LearningPreference } from "@/lib/training-needs/types"
import { learningPreferenceOptions } from "@/lib/training-needs/logic"
import { Section } from "./section"

export interface EmployeeVoiceState {
  skillToImprove: string
  taskNeedingSupport: string
  preferences: LearningPreference[]
  futureSkill: string
}

interface EmployeeVoiceProps {
  value: EmployeeVoiceState
  onChange: (value: EmployeeVoiceState) => void
}

export function EmployeeVoice({ value, onChange }: EmployeeVoiceProps) {
  const togglePreference = (pref: LearningPreference) => {
    const next = value.preferences.includes(pref)
      ? value.preferences.filter((p) => p !== pref)
      : [...value.preferences, pref]
    onChange({ ...value, preferences: next })
  }

  return (
    <Section
      id="section-voice"
      eyebrow="القسم 03"
      title="صوت الموظف"
      description="مدخلاتك النوعية تساعد المنظومة على تخصيص التوصية بدقة أكبر."
    >
      <Card className="grid gap-6 p-5 sm:p-6 md:grid-cols-2">
        <div className="space-y-2">
          <Label htmlFor="skill-improve">ما المهارة التي سيؤدي تطويرها إلى تحسين أدائك؟</Label>
          <Textarea
            id="skill-improve"
            value={value.skillToImprove}
            onChange={(e) => onChange({ ...value, skillToImprove: e.target.value })}
            placeholder="اكتب إجابتك هنا..."
            rows={2}
          />
        </div>
        <div className="space-y-2">
          <Label htmlFor="task-support">ما أكثر مهمة تحتاج فيها إلى دعم؟</Label>
          <Textarea
            id="task-support"
            value={value.taskNeedingSupport}
            onChange={(e) => onChange({ ...value, taskNeedingSupport: e.target.value })}
            placeholder="اكتب إجابتك هنا..."
            rows={2}
          />
        </div>
        <div className="space-y-2 md:col-span-2">
          <Label>ما نوع التعلم الذي تفضله؟</Label>
          <div className="flex flex-wrap gap-2">
            {learningPreferenceOptions.map((opt) => {
              const active = value.preferences.includes(opt.value)
              return (
                <button
                  key={opt.value}
                  type="button"
                  aria-pressed={active}
                  onClick={() => togglePreference(opt.value)}
                  className={cn(
                    "rounded-lg border px-3.5 py-1.5 text-sm font-medium transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring",
                    active
                      ? "border-primary bg-primary text-primary-foreground"
                      : "border-border bg-card text-muted-foreground hover:border-primary/40",
                  )}
                >
                  {opt.label}
                </button>
              )
            })}
          </div>
        </div>
        <div className="space-y-2 md:col-span-2">
          <Label htmlFor="future-skill">هل توجد مهارة مستقبلية تتوقع حاجتك إليها؟</Label>
          <Textarea
            id="future-skill"
            value={value.futureSkill}
            onChange={(e) => onChange({ ...value, futureSkill: e.target.value })}
            placeholder="اكتب إجابتك هنا..."
            rows={2}
          />
        </div>
      </Card>
    </Section>
  )
}
