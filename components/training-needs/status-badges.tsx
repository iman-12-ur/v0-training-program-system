import { cn } from "@/lib/utils"
import type { SkillPriority, SkillStatus } from "@/lib/training-needs/types"
import { priorityLabels } from "@/lib/training-needs/logic"

const priorityStyles: Record<SkillPriority, string> = {
  high: "bg-destructive/10 text-destructive ring-destructive/20",
  medium: "bg-warning/10 text-warning ring-warning/20",
  low: "bg-success/10 text-success ring-success/20",
}

export function PriorityBadge({ priority }: { priority: SkillPriority }) {
  return (
    <span
      className={cn(
        "inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ring-1 ring-inset",
        priorityStyles[priority],
      )}
    >
      {priorityLabels[priority]}
    </span>
  )
}

export function SkillStatusBadge({ status }: { status: SkillStatus }) {
  return (
    <span
      className={cn(
        "inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ring-1 ring-inset",
        status === "complete"
          ? "bg-success/10 text-success ring-success/20"
          : "bg-info/10 text-info ring-info/20",
      )}
    >
      {status === "complete" ? "مكتملة" : "تحتاج تطوير"}
    </span>
  )
}
