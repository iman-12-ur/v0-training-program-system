import { Card } from "@/components/ui/card"
import { ArrowLeft, Lightbulb } from "lucide-react"
import type { SkillAnalysis } from "@/lib/training-needs/types"
import { gapCategoryLabels } from "@/lib/training-needs/logic"
import { Section } from "./section"

interface RecommendationEngineProps {
  gaps: SkillAnalysis[]
}

export function RecommendationEngine({ gaps }: RecommendationEngineProps) {
  return (
    <Section
      id="section-recommendation"
      eyebrow="القسم 07"
      title="محرك التوصية"
      description="لكل فجوة تدخل مقترح مبني على نوع الفجوة وسببها — وكل توصية لها سبب واضح."
    >
      {gaps.length === 0 ? (
        <Card className="p-8 text-center text-sm text-muted-foreground">
          لا توجد توصيات مطلوبة حالياً — مهاراتك مكتملة.
        </Card>
      ) : (
        <div className="grid gap-4">
          {gaps.map((analysis) => (
            <Card key={analysis.skill.id} className="overflow-hidden p-0">
              <div className="grid gap-0 md:grid-cols-[1.1fr_auto_1.4fr]">
                {/* الفجوة */}
                <div className="p-5">
                  <p className="mb-2 text-xs font-semibold uppercase tracking-wider text-muted-foreground">
                    الفجوة
                  </p>
                  <h3 className="text-lg font-bold text-foreground">{analysis.skill.name}</h3>
                  <div className="mt-3 flex flex-wrap items-center gap-x-4 gap-y-1 text-sm">
                    <span className="text-muted-foreground">
                      المطلوب <span className="font-semibold text-foreground">{analysis.skill.requiredLevel} / 5</span>
                    </span>
                    <span className="text-muted-foreground">
                      الحالي <span className="font-semibold text-foreground">{analysis.skill.currentLevel} / 5</span>
                    </span>
                    <span className="text-muted-foreground">
                      الفجوة <span className="font-semibold text-destructive">{analysis.gap}</span>
                    </span>
                  </div>
                  <p className="mt-3 inline-flex rounded-full bg-muted px-2.5 py-0.5 text-xs font-medium text-foreground">
                    نوع الفجوة: {gapCategoryLabels[analysis.skill.category]}
                  </p>
                </div>

                <div className="hidden items-center justify-center bg-muted/30 px-4 md:flex">
                  <ArrowLeft className="h-5 w-5 text-muted-foreground" aria-hidden="true" />
                </div>

                {/* التوصية */}
                <div className="border-t border-border bg-primary/5 p-5 md:border-t-0 md:border-r">
                  <p className="mb-2 flex items-center gap-1.5 text-xs font-semibold uppercase tracking-wider text-primary">
                    <Lightbulb className="h-3.5 w-3.5" />
                    التدخل المقترح
                  </p>
                  <p className="text-lg font-bold text-foreground">{analysis.intervention}</p>
                  <p className="mt-2 text-sm leading-relaxed text-muted-foreground">
                    <span className="font-medium text-foreground">لماذا؟ </span>
                    {analysis.interventionReason}
                  </p>
                </div>
              </div>
            </Card>
          ))}
        </div>
      )}
    </Section>
  )
}
