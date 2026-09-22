"use client"

import { useState } from "react"
import { Card } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog"
import { Check, CircleDot, Circle, CheckCircle2, PencilLine } from "lucide-react"
import { cn } from "@/lib/utils"
import type { LearningPathStep } from "@/lib/training-needs/types"
import { Section } from "./section"

interface LearningPathProps {
  steps: LearningPathStep[]
}

const statusMeta = {
  completed: { label: "مكتمل", dot: "bg-success text-success-foreground", ring: "ring-success/30" },
  proposed: { label: "مقترح", dot: "bg-primary text-primary-foreground", ring: "ring-primary/30" },
  upcoming: { label: "قادم", dot: "bg-muted text-muted-foreground", ring: "ring-border" },
} as const

export function LearningPath({ steps }: LearningPathProps) {
  const [modalOpen, setModalOpen] = useState(false)
  const [approved, setApproved] = useState(false)

  const confirmApproval = () => {
    setApproved(true)
    setModalOpen(false)
  }

  return (
    <Section
      id="section-path"
      eyebrow="القسم 08"
      title="مساري المقترح"
      description="مسار تعلم متكامل يربط التشخيص بالتطبيق ثم قياس الأثر — وليس دورة واحدة."
    >
      <Card className="p-5 sm:p-8">
        <ol className="relative space-y-6 pr-2">
          {/* الخط العمودي للتايم لاين على اليمين (RTL) */}
          <span className="absolute bottom-4 right-[18px] top-4 w-px bg-border" aria-hidden="true" />
          {steps.map((step) => {
            const meta = statusMeta[step.status]
            return (
              <li key={step.order} className="relative flex gap-4">
                <span
                  className={cn(
                    "z-10 flex h-9 w-9 shrink-0 items-center justify-center rounded-full ring-4 ring-background",
                    meta.dot,
                  )}
                >
                  {step.status === "completed" ? (
                    <Check className="h-4 w-4" />
                  ) : step.status === "proposed" ? (
                    <CircleDot className="h-4 w-4" />
                  ) : (
                    <Circle className="h-4 w-4" />
                  )}
                </span>
                <div
                  className={cn(
                    "flex-1 rounded-xl border bg-card p-4 ring-1 ring-inset",
                    meta.ring,
                  )}
                >
                  <div className="flex items-center justify-between gap-3">
                    <div>
                      <span className="text-xs font-medium text-muted-foreground">المرحلة {step.order}</span>
                      <h3 className="text-sm font-semibold text-foreground">{step.title}</h3>
                    </div>
                    <span
                      className={cn(
                        "shrink-0 rounded-full px-2.5 py-0.5 text-xs font-medium",
                        step.status === "completed"
                          ? "bg-success/10 text-success"
                          : step.status === "proposed"
                            ? "bg-primary/10 text-primary"
                            : "bg-muted text-muted-foreground",
                      )}
                    >
                      {meta.label}
                    </span>
                  </div>
                </div>
              </li>
            )
          })}
        </ol>

        <div className="mt-8 flex flex-col gap-3 border-t border-border pt-6 sm:flex-row sm:justify-end">
          {approved ? (
            <div className="flex items-center gap-2 rounded-lg bg-success/10 px-4 py-2.5 text-sm font-medium text-success">
              <CheckCircle2 className="h-5 w-5" />
              تم اعتماد مسارك بنجاح
            </div>
          ) : (
            <>
              <Button variant="outline" className="gap-2 bg-transparent">
                <PencilLine className="h-4 w-4" />
                تعديل المسار
              </Button>
              <Button className="gap-2" onClick={() => setModalOpen(true)}>
                <Check className="h-4 w-4" />
                اعتماد مساري
              </Button>
            </>
          )}
        </div>
      </Card>

      <Dialog open={modalOpen} onOpenChange={setModalOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>اعتماد مسار التعلم</DialogTitle>
            <DialogDescription>هل تريد اعتماد مسار التعلم المقترح؟ سيتم إرساله للمتابعة والتطبيق.</DialogDescription>
          </DialogHeader>
          <DialogFooter className="gap-2 sm:gap-2">
            <Button variant="outline" onClick={() => setModalOpen(false)} className="bg-transparent">
              مراجعة
            </Button>
            <Button onClick={confirmApproval}>اعتماد المسار</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </Section>
  )
}
