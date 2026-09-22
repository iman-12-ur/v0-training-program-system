"use client"

import { useCallback, useEffect, useMemo, useState } from "react"
import { CalendarClock, GraduationCap } from "lucide-react"
import {
  employee,
  initialSkills,
  initialTasks,
  trainingHistory,
  impactStages,
  baseLearningPath,
  lastAssessmentDate,
} from "@/lib/training-needs/data"
import { analyzeSkill, summarizeGaps, calculateReadiness } from "@/lib/training-needs/logic"
import type {
  ChallengeLevel,
  GapDiagnosis,
  JobTask,
  ObstacleType,
  Skill,
} from "@/lib/training-needs/types"
import { JourneyStepper, journeySteps, type JourneyStep } from "@/components/training-needs/journey-stepper"
import { EmployeeProfile } from "@/components/training-needs/employee-profile"
import { JobTasks } from "@/components/training-needs/job-tasks"
import { SkillsAssessment } from "@/components/training-needs/skills-assessment"
import { EmployeeVoice, type EmployeeVoiceState } from "@/components/training-needs/employee-voice"
import { SuggestForMe } from "@/components/training-needs/suggest-for-me"
import { GapAnalysis } from "@/components/training-needs/gap-analysis"
import { GapCauseAnalysis } from "@/components/training-needs/gap-cause-analysis"
import { RecommendationEngine } from "@/components/training-needs/recommendation-engine"
import { LearningPath } from "@/components/training-needs/learning-path"
import { PreviousLearning } from "@/components/training-needs/previous-learning"
import { ImpactMeasurement } from "@/components/training-needs/impact-measurement"
import { Pdca } from "@/components/training-needs/pdca"
import { SkillReadiness } from "@/components/training-needs/skill-readiness"

export default function TrainingNeedsPage() {
  const [skills, setSkills] = useState<Skill[]>(initialSkills)
  const [tasks, setTasks] = useState<JobTask[]>(initialTasks)
  const [hardestTaskId, setHardestTaskId] = useState<string>(initialTasks[0]?.id ?? "")
  const [obstacle, setObstacle] = useState<ObstacleType | "">("")
  const [voice, setVoice] = useState<EmployeeVoiceState>({
    skillToImprove: "",
    taskNeedingSupport: "",
    preferences: [],
    futureSkill: "",
  })
  const [diagnoses, setDiagnoses] = useState<Record<string, GapDiagnosis>>({})
  const [activeStep, setActiveStep] = useState<string>("profile")

  // إعادة الحساب تلقائياً عند أي تغيير في المستويات.
  const analyses = useMemo(() => skills.map(analyzeSkill), [skills])
  const summary = useMemo(() => summarizeGaps(analyses), [analyses])
  const readiness = useMemo(() => calculateReadiness(skills), [skills])
  const gaps = useMemo(
    () => analyses.filter((a) => a.status === "gap").sort((a, b) => b.gap - a.gap),
    [analyses],
  )
  const topGap = gaps[0] ?? null

  const handleLevelChange = useCallback((skillId: string, level: number) => {
    setSkills((prev) => prev.map((s) => (s.id === skillId ? { ...s, currentLevel: level } : s)))
  }, [])

  const handleChallengeChange = useCallback((taskId: string, challenge: ChallengeLevel) => {
    setTasks((prev) => prev.map((t) => (t.id === taskId ? { ...t, challenge } : t)))
  }, [])

  const handleDiagnosisChange = useCallback((skillId: string, diagnosis: GapDiagnosis) => {
    setDiagnoses((prev) => ({ ...prev, [skillId]: diagnosis }))
  }, [])

  const handleStepSelect = useCallback((step: JourneyStep) => {
    document.getElementById(step.targetId)?.scrollIntoView({ behavior: "smooth", block: "start" })
  }, [])

  // تتبّع القسم الظاهر لتحديث الـ Stepper (Scroll Spy).
  useEffect(() => {
    const sections = journeySteps
      .map((step) => {
        const el = document.getElementById(step.targetId)
        return el ? { id: step.id, el } : null
      })
      .filter((v): v is { id: string; el: HTMLElement } => v !== null)

    const observer = new IntersectionObserver(
      (entries) => {
        const visible = entries
          .filter((e) => e.isIntersecting)
          .sort((a, b) => b.intersectionRatio - a.intersectionRatio)[0]
        if (visible) {
          const match = sections.find((s) => s.el === visible.target)
          if (match) setActiveStep(match.id)
        }
      },
      { rootMargin: "-20% 0px -60% 0px", threshold: [0.1, 0.25, 0.5] },
    )

    sections.forEach((s) => observer.observe(s.el))
    return () => observer.disconnect()
  }, [])

  return (
    <div className="min-h-screen bg-background">
      {/* شريط علوي بهوية المنظومة */}
      <header className="bg-brand text-brand-foreground">
        <div className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
          <div className="flex flex-col gap-6 lg:flex-row lg:items-center lg:justify-between">
            <div>
              <div className="mb-3 flex items-center gap-2.5">
                <span className="flex h-9 w-9 items-center justify-center rounded-xl bg-white/10">
                  <GraduationCap className="h-5 w-5" />
                </span>
                <span className="text-sm font-medium text-white/80">مساري — ذكاء المهارات والتعلم المؤسسي</span>
              </div>
              <h1 className="text-balance text-2xl font-bold sm:text-3xl">الاحتياجات التدريبية</h1>
              <p className="mt-2 max-w-2xl text-pretty text-sm text-white/80 sm:text-base">
                اكتشف فجوات مهاراتك واحصل على مسار تعلم مناسب لدورك الوظيفي.
              </p>
            </div>
            <div className="flex items-center gap-2.5 rounded-xl bg-white/10 px-4 py-3 text-sm ring-1 ring-inset ring-white/15">
              <CalendarClock className="h-5 w-5 text-white/80" />
              <div>
                <p className="text-white/70">آخر تحديث للتقييم</p>
                <p className="font-semibold">{lastAssessmentDate}</p>
              </div>
            </div>
          </div>
        </div>
      </header>

      {/* شريط المراحل الثابت */}
      <div className="sticky top-0 z-30 border-b border-border bg-background/85 backdrop-blur supports-[backdrop-filter]:bg-background/70">
        <div className="mx-auto max-w-7xl px-4 py-3 sm:px-6 lg:px-8">
          <JourneyStepper activeId={activeStep} onSelect={handleStepSelect} />
        </div>
      </div>

      {/* المحتوى */}
      <main className="mx-auto max-w-7xl px-4 py-10 sm:px-6 lg:px-8">
        <div className="grid gap-10 xl:grid-cols-[1fr_300px]">
          <div className="min-w-0 space-y-14">
            <EmployeeProfile
              employee={employee}
              requiredSkills={skills.length}
              currentGaps={summary.totalSkills - summary.completed}
            />
            <SkillsAssessment analyses={analyses} onLevelChange={handleLevelChange} />
            <JobTasks
              tasks={tasks}
              onChallengeChange={handleChallengeChange}
              hardestTaskId={hardestTaskId}
              onHardestTaskChange={setHardestTaskId}
              obstacle={obstacle}
              onObstacleChange={setObstacle}
            />
            <EmployeeVoice value={voice} onChange={setVoice} />
            <SuggestForMe topGap={topGap} />
            <GapAnalysis analyses={analyses} summary={summary} />
            <GapCauseAnalysis gaps={gaps} diagnoses={diagnoses} onDiagnosisChange={handleDiagnosisChange} />
            <RecommendationEngine gaps={gaps} />
            <LearningPath steps={baseLearningPath} />
            <PreviousLearning history={trainingHistory} skills={skills} />
            <ImpactMeasurement stages={impactStages} />
            <Pdca />
          </div>

          {/* لوحة جانبية ثابتة */}
          <aside className="hidden xl:block">
            <div className="sticky top-24 space-y-6">
              <SkillReadiness readiness={readiness} />
            </div>
          </aside>
        </div>
      </main>
    </div>
  )
}
