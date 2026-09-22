"use client"

import { Card } from "@/components/ui/card"
import { Label } from "@/components/ui/label"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import type { GapDiagnosis, SkillAnalysis } from "@/lib/training-needs/types"
import { Section } from "./section"

interface GapCauseAnalysisProps {
  gaps: SkillAnalysis[]
  diagnoses: Record<string, GapDiagnosis>
  onDiagnosisChange: (skillId: string, diagnosis: GapDiagnosis) => void
}

const diagnosisOptions: { value: GapDiagnosis; label: string }[] = [
  { value: "insufficient-knowledge", label: "معرفة غير كافية" },
  { value: "incomplete-skill", label: "مهارة غير مكتملة" },
  { value: "weak-application", label: "ضعف في التطبيق" },
  { value: "lack-experience", label: "قلة الخبرة العملية" },
  { value: "tool-system", label: "أداة / نظام" },
  { value: "unclear", label: "غير واضح" },
]

export function GapCauseAnalysis({ gaps, diagnoses, onDiagnosisChange }: GapCauseAnalysisProps) {
  return (
    <Section
      id="section-cause"
      eyebrow="القسم 06"
      title="تحليل سبب الفجوة"
      description="لا نكتفي برقم الفجوة — نحدد سببها أولاً لاختيار التدخل الأنسب."
    >
      {gaps.length === 0 ? (
        <Card className="p-8 text-center text-sm text-muted-foreground">
          لا توجد فجوات حالياً. جميع مهاراتك عند المستوى المطلوب.
        </Card>
      ) : (
        <div className="grid gap-4 md:grid-cols-2">
          {gaps.map(({ skill, gap }) => (
            <Card key={skill.id} className="space-y-4 p-5">
              <div className="flex items-center justify-between">
                <h3 className="text-base font-semibold text-foreground">{skill.name}</h3>
                <span className="rounded-full bg-destructive/10 px-2.5 py-0.5 text-xs font-medium text-destructive ring-1 ring-inset ring-destructive/20">
                  فجوة {gap}
                </span>
              </div>
              <div className="grid grid-cols-2 gap-3">
                <div className="rounded-lg bg-muted/40 p-3">
                  <p className="text-xs text-muted-foreground">المطلوب</p>
                  <p className="text-lg font-bold text-foreground">{skill.requiredLevel} / 5</p>
                </div>
                <div className="rounded-lg bg-muted/40 p-3">
                  <p className="text-xs text-muted-foreground">الحالي</p>
                  <p className="text-lg font-bold text-foreground">{skill.currentLevel} / 5</p>
                </div>
              </div>
              <div className="space-y-2">
                <Label htmlFor={`diagnosis-${skill.id}`} className="text-sm font-medium">
                  التشخيص الأولي
                </Label>
                <Select
                  value={diagnoses[skill.id] ?? ""}
                  onValueChange={(v) => onDiagnosisChange(skill.id, v as GapDiagnosis)}
                >
                  <SelectTrigger id={`diagnosis-${skill.id}`}>
                    <SelectValue placeholder="اختر التشخيص" />
                  </SelectTrigger>
                  <SelectContent>
                    {diagnosisOptions.map((opt) => (
                      <SelectItem key={opt.value} value={opt.value}>
                        {opt.label}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
            </Card>
          ))}
        </div>
      )}
    </Section>
  )
}
