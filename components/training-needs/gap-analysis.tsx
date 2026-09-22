"use client"

import { Bar, BarChart, CartesianGrid, XAxis, YAxis } from "recharts"
import { Card } from "@/components/ui/card"
import { ChartContainer, ChartTooltip, ChartTooltipContent, type ChartConfig } from "@/components/ui/chart"
import { Layers, TrendingDown, Minus, CircleCheck } from "lucide-react"
import type { GapSummary } from "@/lib/training-needs/logic"
import type { SkillAnalysis } from "@/lib/training-needs/types"
import { Section } from "./section"
import { cn } from "@/lib/utils"

interface GapAnalysisProps {
  analyses: SkillAnalysis[]
  summary: GapSummary
}

const chartConfig = {
  required: { label: "المستوى المطلوب", color: "var(--brand)" },
  current: { label: "المستوى الحالي", color: "var(--primary)" },
} satisfies ChartConfig

export function GapAnalysis({ analyses, summary }: GapAnalysisProps) {
  const kpis = [
    { icon: Layers, label: "إجمالي المهارات", value: summary.totalSkills, tone: "text-info bg-info/10" },
    { icon: TrendingDown, label: "فجوات عالية", value: summary.highGaps, tone: "text-destructive bg-destructive/10" },
    { icon: Minus, label: "فجوات متوسطة", value: summary.mediumGaps, tone: "text-warning bg-warning/10" },
    { icon: CircleCheck, label: "مهارات مكتملة", value: summary.completed, tone: "text-success bg-success/10" },
  ]

  const chartData = analyses.map((a) => ({
    skill: a.skill.name,
    required: a.skill.requiredLevel,
    current: a.skill.currentLevel,
  }))

  return (
    <Section
      id="section-gaps"
      eyebrow="القسم 05"
      title="تحليل فجوات مهاراتي"
      description="نظرة شاملة على فجواتك مقارنةً بالمستويات المطلوبة لوظيفتك."
    >
      <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
        {kpis.map(({ icon: Icon, label, value, tone }) => (
          <Card key={label} className="p-5">
            <div className={cn("mb-3 flex h-10 w-10 items-center justify-center rounded-xl", tone)}>
              <Icon className="h-5 w-5" />
            </div>
            <p className="text-3xl font-bold text-foreground">{value}</p>
            <p className="mt-1 text-sm text-muted-foreground">{label}</p>
          </Card>
        ))}
      </div>

      <Card className="mt-6 p-5 sm:p-6">
        <h3 className="mb-1 text-base font-semibold text-foreground">المستوى المطلوب مقابل الحالي</h3>
        <p className="mb-4 text-sm text-muted-foreground">لكل مهارة على مقياس من 1 إلى 5.</p>
        <ChartContainer config={chartConfig} className="h-[320px] w-full">
          <BarChart accessibilityLayer data={chartData} margin={{ top: 8, right: 8, left: 8, bottom: 8 }}>
            <CartesianGrid vertical={false} strokeDasharray="3 3" />
            <XAxis
              dataKey="skill"
              tickLine={false}
              axisLine={false}
              tickMargin={8}
              interval={0}
              tick={{ fontSize: 11 }}
              reversed
            />
            <YAxis
              domain={[0, 5]}
              ticks={[0, 1, 2, 3, 4, 5]}
              tickLine={false}
              axisLine={false}
              width={24}
              orientation="right"
            />
            <ChartTooltip content={<ChartTooltipContent />} />
            <Bar dataKey="required" fill="var(--color-required)" radius={[4, 4, 0, 0]} />
            <Bar dataKey="current" fill="var(--color-current)" radius={[4, 4, 0, 0]} />
          </BarChart>
        </ChartContainer>
        <div className="mt-4 flex flex-wrap items-center justify-center gap-6">
          <LegendDot color="var(--brand)" label="المستوى المطلوب" />
          <LegendDot color="var(--primary)" label="المستوى الحالي" />
        </div>
      </Card>
    </Section>
  )
}

function LegendDot({ color, label }: { color: string; label: string }) {
  return (
    <span className="flex items-center gap-2 text-sm text-muted-foreground">
      <span className="h-3 w-3 rounded-sm" style={{ backgroundColor: color }} aria-hidden="true" />
      {label}
    </span>
  )
}
