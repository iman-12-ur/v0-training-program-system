import type { ReactNode } from "react"
import { cn } from "@/lib/utils"

interface SectionProps {
  id?: string
  eyebrow?: string
  title: string
  description?: string
  action?: ReactNode
  children: ReactNode
  className?: string
}

// غلاف موحّد لكل قسم في الصفحة لضمان اتساق العناوين والمسافات.
export function Section({ id, eyebrow, title, description, action, children, className }: SectionProps) {
  return (
    <section id={id} className={cn("scroll-mt-24", className)} aria-labelledby={id ? `${id}-title` : undefined}>
      <div className="mb-5 flex flex-col gap-3 sm:flex-row sm:items-end sm:justify-between">
        <div className="space-y-1">
          {eyebrow ? (
            <span className="text-xs font-semibold uppercase tracking-wider text-primary">{eyebrow}</span>
          ) : null}
          <h2 id={id ? `${id}-title` : undefined} className="text-pretty text-xl font-bold text-foreground sm:text-2xl">
            {title}
          </h2>
          {description ? <p className="max-w-2xl text-pretty text-sm text-muted-foreground">{description}</p> : null}
        </div>
        {action ? <div className="shrink-0">{action}</div> : null}
      </div>
      {children}
    </section>
  )
}
