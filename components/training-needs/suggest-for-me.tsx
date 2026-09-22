"use client"

import { useEffect, useRef, useState } from "react"
import { Card } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Sparkles, Loader2, Check, Wand2 } from "lucide-react"
import { cn } from "@/lib/utils"
import type { SkillAnalysis } from "@/lib/training-needs/types"
import { Section } from "./section"

interface SuggestForMeProps {
  topGap: SkillAnalysis | null
}

const analysisSteps = [
  "تم تحليل بيانات الوظيفة",
  "تم تحليل فجوات المهارات",
  "تمت مراجعة سجل التعلم السابق",
  "جاري بناء المسار المقترح",
]

export function SuggestForMe({ topGap }: SuggestForMeProps) {
  const [phase, setPhase] = useState<"idle" | "running" | "done">("idle")
  const [completedSteps, setCompletedSteps] = useState(0)
  const timers = useRef<ReturnType<typeof setTimeout>[]>([])

  useEffect(() => {
    return () => timers.current.forEach(clearTimeout)
  }, [])

  const runAnalysis = () => {
    timers.current.forEach(clearTimeout)
    timers.current = []
    setPhase("running")
    setCompletedSteps(0)

    analysisSteps.forEach((_, index) => {
      const t = setTimeout(() => {
        setCompletedSteps(index + 1)
        if (index === analysisSteps.length - 1) {
          const done = setTimeout(() => setPhase("done"), 700)
          timers.current.push(done)
        }
      }, (index + 1) * 900)
      timers.current.push(t)
    })
  }

  return (
    <Section id="section-suggest" eyebrow="القسم 04" title="لا أعرف – اقترح لي">
      <Card className="overflow-hidden border-primary/30 bg-gradient-to-l from-primary/5 to-transparent p-6 sm:p-8">
        <div className="flex flex-col items-start gap-6 lg:flex-row lg:items-center lg:justify-between">
          <div className="flex items-start gap-4">
            <div className="flex h-12 w-12 shrink-0 items-center justify-center rounded-2xl bg-primary text-primary-foreground">
              <Wand2 className="h-6 w-6" />
            </div>
            <div className="max-w-xl">
              <h3 className="text-lg font-bold text-foreground">لا تعرف ما الذي تحتاجه؟</h3>
              <p className="mt-1.5 text-sm leading-relaxed text-muted-foreground">
                لا تقلق، ستقوم المنظومة بتحليل وظيفتك ومهاراتك وفجواتك وسجل تعلمك السابق لتقترح لك التدخل الأنسب.
              </p>
            </div>
          </div>
          <Button size="lg" onClick={runAnalysis} disabled={phase === "running"} className="gap-2">
            {phase === "running" ? <Loader2 className="h-4 w-4 animate-spin" /> : <Sparkles className="h-4 w-4" />}
            {phase === "running" ? "جاري التحليل..." : "لا أعرف – اقترح لي"}
          </Button>
        </div>

        {phase !== "idle" ? (
          <div className="mt-6 border-t border-border/60 pt-6">
            {phase === "running" ? (
              <p className="mb-4 text-sm font-medium text-foreground">جاري تحليل احتياجاتك...</p>
            ) : null}
            <ul className="grid gap-2.5 sm:grid-cols-2">
              {analysisSteps.map((step, index) => {
                const active = index < completedSteps
                return (
                  <li
                    key={step}
                    className={cn(
                      "flex items-center gap-2.5 rounded-lg px-3 py-2 text-sm transition-colors",
                      active ? "bg-success/10 text-foreground" : "bg-muted/40 text-muted-foreground",
                    )}
                  >
                    <span
                      className={cn(
                        "flex h-5 w-5 shrink-0 items-center justify-center rounded-full",
                        active ? "bg-success text-success-foreground" : "bg-muted",
                      )}
                    >
                      {active ? <Check className="h-3 w-3" /> : <Loader2 className="h-3 w-3 animate-spin" />}
                    </span>
                    {step}
                  </li>
                )
              })}
            </ul>

            {phase === "done" ? (
              <div className="mt-5 rounded-xl border border-primary/30 bg-card p-5">
                <p className="flex items-center gap-2 text-sm font-semibold text-primary">
                  <Sparkles className="h-4 w-4" />
                  التوصية المقترحة
                </p>
                {topGap ? (
                  <div className="mt-3">
                    <p className="text-base font-bold text-foreground">
                      ابدأ بمهارة {topGap.skill.name} — {topGap.intervention}
                    </p>
                    <p className="mt-1.5 text-sm text-muted-foreground">{topGap.interventionReason}</p>
                    <p className="mt-3 text-sm text-muted-foreground">
                      اطّلع على المسار الكامل في قسم <span className="font-medium text-foreground">«مساري المقترح»</span> أدناه.
                    </p>
                  </div>
                ) : (
                  <p className="mt-3 text-sm text-muted-foreground">
                    مهاراتك عند المستوى المطلوب حالياً — نوصي بالتركيز على التطبيق وقياس الأثر.
                  </p>
                )}
              </div>
            ) : null}
          </div>
        ) : null}
      </Card>
    </Section>
  )
}
