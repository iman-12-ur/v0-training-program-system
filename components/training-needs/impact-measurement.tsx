import { Card } from "@/components/ui/card"
import { Progress } from "@/components/ui/progress"
import type { ImpactStage } from "@/lib/training-needs/types"
import { Section } from "./section"

interface ImpactMeasurementProps {
  stages: ImpactStage[]
}

export function ImpactMeasurement({ stages }: ImpactMeasurementProps) {
  return (
    <Section
      id="section-impact"
      eyebrow="القسم 11"
      title="قياس أثر التعلم"
      description="نقيس الأثر على أربع مراحل — من رضا المتعلم إلى الأثر على مؤشر الأداء."
    >
      <div className="grid gap-4 sm:grid-cols-2">
        {stages.map((stage) => (
          <Card key={stage.order} className="space-y-3 p-5">
            <div className="flex items-start gap-3">
              <span className="flex h-8 w-8 shrink-0 items-center justify-center rounded-lg bg-primary/10 text-sm font-bold text-primary">
                {String(stage.order).padStart(2, "0")}
              </span>
              <div>
                <h3 className="text-sm font-semibold text-foreground">{stage.title}</h3>
                {stage.description ? (
                  <p className="mt-0.5 text-xs text-muted-foreground">{stage.description}</p>
                ) : null}
              </div>
            </div>
            <div className="space-y-1.5">
              <div className="flex items-center justify-between text-xs">
                <span className="text-muted-foreground">نسبة الإنجاز</span>
                <span className="font-semibold text-foreground">{stage.progress}%</span>
              </div>
              <Progress value={stage.progress} className="h-2" />
            </div>
          </Card>
        ))}
      </div>
    </Section>
  )
}
