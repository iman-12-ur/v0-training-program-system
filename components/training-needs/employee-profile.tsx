import { Card } from "@/components/ui/card"
import { Badge } from "@/components/ui/badge"
import { Building2, BriefcaseBusiness, Layers, UserCog, Target, AlertTriangle, Sparkles } from "lucide-react"
import type { Employee } from "@/lib/training-needs/types"
import { Section } from "./section"

interface EmployeeProfileProps {
  employee: Employee
  requiredSkills: number
  currentGaps: number
}

export function EmployeeProfile({ employee, requiredSkills, currentGaps }: EmployeeProfileProps) {
  const cards = [
    { icon: BriefcaseBusiness, label: "المسمى الوظيفي", value: employee.jobTitle },
    { icon: Building2, label: "الدائرة", value: employee.department },
    { icon: Layers, label: "القسم", value: employee.unit },
    { icon: UserCog, label: "المدير المباشر", value: employee.directManager },
    { icon: Target, label: "المهارات المطلوبة", value: String(requiredSkills) },
    { icon: AlertTriangle, label: "الفجوات الحالية", value: String(currentGaps) },
  ]

  return (
    <Section
      id="section-profile"
      eyebrow="ملفي الوظيفي"
      title="ملفي الوظيفي"
      description="بياناتك الوظيفية كما هي مسجلة في المنظومة — لا حاجة لإعادة إدخالها."
      action={
        employee.autoFetched ? (
          <Badge className="gap-1.5 bg-info/10 text-info ring-1 ring-inset ring-info/20 hover:bg-info/15">
            <Sparkles className="h-3.5 w-3.5" />
            بيانات تم جلبها تلقائياً
          </Badge>
        ) : null
      }
    >
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {cards.map(({ icon: Icon, label, value }) => (
          <Card key={label} className="flex items-center gap-4 p-5">
            <div className="flex h-11 w-11 shrink-0 items-center justify-center rounded-xl bg-primary/10 text-primary">
              <Icon className="h-5 w-5" />
            </div>
            <div className="min-w-0">
              <p className="text-xs text-muted-foreground">{label}</p>
              <p className="truncate text-base font-semibold text-foreground">{value}</p>
            </div>
          </Card>
        ))}
      </div>
    </Section>
  )
}
