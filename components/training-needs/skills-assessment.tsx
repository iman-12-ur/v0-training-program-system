"use client"

import { Card } from "@/components/ui/card"
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table"
import type { SkillAnalysis } from "@/lib/training-needs/types"
import { skillLevelLabel } from "@/lib/training-needs/logic"
import { Section } from "./section"
import { SkillLevelSelector } from "./skill-level-selector"
import { PriorityBadge, SkillStatusBadge } from "./status-badges"

interface SkillsAssessmentProps {
  analyses: SkillAnalysis[]
  onLevelChange: (skillId: string, level: number) => void
}

function GapPips({ gap }: { gap: number }) {
  return (
    <div className="flex items-center gap-2">
      <div className="flex gap-1" aria-hidden="true">
        {[1, 2, 3, 4].map((n) => (
          <span
            key={n}
            className={`h-2 w-2 rounded-full ${n <= gap ? "bg-destructive" : "bg-muted"}`}
          />
        ))}
      </div>
      <span className="text-sm font-semibold text-foreground">{gap}</span>
    </div>
  )
}

export function SkillsAssessment({ analyses, onLevelChange }: SkillsAssessmentProps) {
  return (
    <Section
      id="section-skills"
      eyebrow="القسم 02"
      title="تقييم مهاراتي"
      description="قارن مستواك الحالي بالمستوى المطلوب لأداء وظيفتك. عند تعديل المستوى تُعاد الحسابات تلقائياً."
    >
      {/* عرض جدولي للشاشات الكبيرة */}
      <Card className="hidden overflow-hidden p-0 lg:block">
        <Table>
          <TableHeader>
            <TableRow className="bg-muted/50">
              <TableHead className="text-right">المهارة</TableHead>
              <TableHead className="text-center">المطلوب</TableHead>
              <TableHead className="text-right">الحالي</TableHead>
              <TableHead className="text-right">الفجوة</TableHead>
              <TableHead className="text-right">الأولوية</TableHead>
              <TableHead className="text-right">الحالة</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {analyses.map(({ skill, gap, priority, status }) => (
              <TableRow key={skill.id}>
                <TableCell className="font-medium text-foreground">{skill.name}</TableCell>
                <TableCell className="text-center">
                  <span className="inline-flex h-7 min-w-7 items-center justify-center rounded-md bg-brand/10 px-2 text-sm font-semibold text-brand">
                    {skill.requiredLevel}
                  </span>
                </TableCell>
                <TableCell>
                  <SkillLevelSelector
                    skillName={skill.name}
                    value={skill.currentLevel}
                    onChange={(v) => onLevelChange(skill.id, v)}
                  />
                </TableCell>
                <TableCell>
                  <GapPips gap={gap} />
                </TableCell>
                <TableCell>
                  <PriorityBadge priority={priority} />
                </TableCell>
                <TableCell>
                  <SkillStatusBadge status={status} />
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </Card>

      {/* عرض بطاقات للشاشات الصغيرة */}
      <div className="grid gap-4 lg:hidden">
        {analyses.map(({ skill, gap, priority, status }) => (
          <Card key={skill.id} className="space-y-4 p-5">
            <div className="flex items-start justify-between gap-3">
              <div>
                <h3 className="font-semibold text-foreground">{skill.name}</h3>
                <p className="mt-0.5 text-xs text-muted-foreground">
                  المطلوب: {skill.requiredLevel} · {skillLevelLabel(skill.requiredLevel)}
                </p>
              </div>
              <SkillStatusBadge status={status} />
            </div>
            <div>
              <span className="mb-1.5 block text-xs font-medium text-muted-foreground">
                مستواك الحالي ({skillLevelLabel(skill.currentLevel)})
              </span>
              <SkillLevelSelector
                skillName={skill.name}
                value={skill.currentLevel}
                onChange={(v) => onLevelChange(skill.id, v)}
              />
            </div>
            <div className="flex items-center justify-between border-t border-border pt-3">
              <div className="flex items-center gap-2">
                <span className="text-xs text-muted-foreground">الفجوة</span>
                <GapPips gap={gap} />
              </div>
              <PriorityBadge priority={priority} />
            </div>
          </Card>
        ))}
      </div>
    </Section>
  )
}
