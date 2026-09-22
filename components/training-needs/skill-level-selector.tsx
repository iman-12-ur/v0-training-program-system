"use client"

import { cn } from "@/lib/utils"
import { SKILL_LEVELS } from "@/lib/training-needs/logic"

interface SkillLevelSelectorProps {
  value: number
  onChange: (value: number) => void
  skillName: string
}

// عنصر تحكم مقسّم لاختيار المستوى الحالي (1..5) مع تسميات واضحة.
export function SkillLevelSelector({ value, onChange, skillName }: SkillLevelSelectorProps) {
  return (
    <div
      role="radiogroup"
      aria-label={`المستوى الحالي لمهارة ${skillName}`}
      className="inline-flex overflow-hidden rounded-lg border border-border"
    >
      {SKILL_LEVELS.map((level) => {
        const active = value === level.value
        return (
          <button
            key={level.value}
            type="button"
            role="radio"
            aria-checked={active}
            title={level.label}
            onClick={() => onChange(level.value)}
            className={cn(
              "flex h-9 w-9 items-center justify-center border-l border-border text-sm font-semibold transition-colors last:border-l-0 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-inset focus-visible:ring-ring",
              active
                ? "bg-primary text-primary-foreground"
                : "bg-card text-muted-foreground hover:bg-muted",
            )}
          >
            {level.value}
          </button>
        )
      })}
    </div>
  )
}
