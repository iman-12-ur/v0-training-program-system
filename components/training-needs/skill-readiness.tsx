import { Card } from "@/components/ui/card"
import { CircularProgress } from "./circular-progress"

interface SkillReadinessProps {
  readiness: number
}

export function SkillReadiness({ readiness }: SkillReadinessProps) {
  return (
    <Card className="flex flex-col items-center gap-4 p-6 text-center">
      <h3 className="text-base font-semibold text-foreground">جاهزية مهاراتي</h3>
      <CircularProgress value={readiness} label="الجاهزية" />
      <p className="text-xs leading-relaxed text-muted-foreground">
        مؤشر تجريبي يعتمد على مقارنة مستويات المهارات الحالية بالمستويات المطلوبة.
      </p>
    </Card>
  )
}
