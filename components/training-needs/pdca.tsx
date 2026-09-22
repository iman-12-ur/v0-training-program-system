import { Card } from "@/components/ui/card"
import { ClipboardList, Play, SearchCheck, RefreshCw, RotateCw } from "lucide-react"
import { Section } from "./section"

const phases = [
  {
    key: "plan",
    title: "PLAN",
    icon: ClipboardList,
    tone: "bg-info/10 text-info",
    items: ["تحديد الفجوة", "الهدف", "مؤشر النجاح"],
  },
  {
    key: "do",
    title: "DO",
    icon: Play,
    tone: "bg-primary/10 text-primary",
    items: ["تنفيذ التعلم", "أو التدريب", "أو التدخل التطويري"],
  },
  {
    key: "check",
    title: "CHECK",
    icon: SearchCheck,
    tone: "bg-warning/10 text-warning",
    items: ["قياس المعرفة", "قياس المهارة", "قياس التطبيق"],
  },
  {
    key: "act",
    title: "ACT",
    icon: RefreshCw,
    tone: "bg-success/10 text-success",
    items: ["تثبيت الحل الناجح", "أو تعديل المسار", "وإعادة التقييم"],
  },
]

export function Pdca() {
  return (
    <Section
      id="section-pdca"
      eyebrow="القسم 12"
      title="التحسين المستمر PDCA"
      description="دورة قابلة للتكرار تضمن تحسّن الأداء باستمرار بعد كل تدخل."
      action={
        <span className="hidden items-center gap-1.5 rounded-full bg-muted px-3 py-1 text-xs font-medium text-muted-foreground sm:inline-flex">
          <RotateCw className="h-3.5 w-3.5" />
          دورة متكررة
        </span>
      }
    >
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        {phases.map((phase, index) => {
          const Icon = phase.icon
          return (
            <Card key={phase.key} className="relative space-y-3 p-5">
              <div className="flex items-center justify-between">
                <div className={`flex h-10 w-10 items-center justify-center rounded-xl ${phase.tone}`}>
                  <Icon className="h-5 w-5" />
                </div>
                <span className="text-xs font-bold text-muted-foreground/60">{`0${index + 1}`}</span>
              </div>
              <h3 className="text-lg font-bold tracking-wide text-foreground">{phase.title}</h3>
              <ul className="space-y-1.5">
                {phase.items.map((item) => (
                  <li key={item} className="flex items-center gap-2 text-sm text-muted-foreground">
                    <span className="h-1.5 w-1.5 rounded-full bg-current opacity-40" aria-hidden="true" />
                    {item}
                  </li>
                ))}
              </ul>
            </Card>
          )
        })}
      </div>
    </Section>
  )
}
