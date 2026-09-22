"use client"

import { cn } from "@/lib/utils"
import { Check } from "lucide-react"

export interface JourneyStep {
  id: string
  order: string
  label: string
  targetId: string
}

export const journeySteps: JourneyStep[] = [
  { id: "profile", order: "01", label: "بياناتي", targetId: "section-profile" },
  { id: "skills", order: "02", label: "مهاراتي", targetId: "section-skills" },
  { id: "challenges", order: "03", label: "تحدياتي", targetId: "section-tasks" },
  { id: "gaps", order: "04", label: "الفجوات", targetId: "section-gaps" },
  { id: "path", order: "05", label: "مساري", targetId: "section-path" },
  { id: "follow", order: "06", label: "المتابعة", targetId: "section-impact" },
]

interface JourneyStepperProps {
  activeId: string
  onSelect: (step: JourneyStep) => void
}

export function JourneyStepper({ activeId, onSelect }: JourneyStepperProps) {
  const activeIndex = journeySteps.findIndex((s) => s.id === activeId)

  return (
    <nav aria-label="مراحل الرحلة التشخيصية">
      <ol className="flex items-center gap-2 overflow-x-auto pb-1 sm:gap-3 sm:overflow-visible [scrollbar-width:none] [&::-webkit-scrollbar]:hidden">
        {journeySteps.map((step, index) => {
          const isActive = index === activeIndex
          const isDone = index < activeIndex
          return (
            <li key={step.id} className="flex min-w-[130px] flex-1 items-center gap-3 sm:min-w-0">
              <button
                type="button"
                onClick={() => onSelect(step)}
                aria-current={isActive ? "step" : undefined}
                className={cn(
                  "flex flex-1 items-center gap-3 rounded-xl border px-3 py-2.5 text-right transition-all",
                  isActive
                    ? "border-primary bg-primary/5 shadow-sm"
                    : isDone
                      ? "border-success/30 bg-success/5 hover:border-success/50"
                      : "border-border bg-card hover:border-primary/40",
                )}
              >
                <span
                  className={cn(
                    "flex h-9 w-9 shrink-0 items-center justify-center rounded-lg text-sm font-bold",
                    isActive
                      ? "bg-primary text-primary-foreground"
                      : isDone
                        ? "bg-success text-success-foreground"
                        : "bg-muted text-muted-foreground",
                  )}
                >
                  {isDone ? <Check className="h-4 w-4" /> : step.order}
                </span>
                <span
                  className={cn(
                    "text-sm font-semibold",
                    isActive ? "text-foreground" : "text-muted-foreground",
                  )}
                >
                  {step.label}
                </span>
              </button>
              {index < journeySteps.length - 1 ? (
                <span className="hidden h-px w-4 shrink-0 bg-border sm:block" aria-hidden="true" />
              ) : null}
            </li>
          )
        })}
      </ol>
    </nav>
  )
}
