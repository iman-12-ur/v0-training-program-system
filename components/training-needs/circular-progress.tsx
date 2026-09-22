import { cn } from "@/lib/utils"

interface CircularProgressProps {
  value: number // 0..100
  size?: number
  strokeWidth?: number
  label?: string
  className?: string
}

// حلقة تقدم دائرية لعرض مؤشر الجاهزية.
export function CircularProgress({
  value,
  size = 160,
  strokeWidth = 12,
  label,
  className,
}: CircularProgressProps) {
  const clamped = Math.min(100, Math.max(0, value))
  const radius = (size - strokeWidth) / 2
  const circumference = 2 * Math.PI * radius
  const offset = circumference - (clamped / 100) * circumference

  return (
    <div className={cn("relative inline-flex items-center justify-center", className)}>
      <svg
        width={size}
        height={size}
        viewBox={`0 0 ${size} ${size}`}
        role="img"
        aria-label={`${label ?? "التقدم"}: ${clamped}%`}
        className="-rotate-90"
      >
        <circle
          cx={size / 2}
          cy={size / 2}
          r={radius}
          fill="none"
          stroke="var(--muted)"
          strokeWidth={strokeWidth}
        />
        <circle
          cx={size / 2}
          cy={size / 2}
          r={radius}
          fill="none"
          stroke="var(--primary)"
          strokeWidth={strokeWidth}
          strokeLinecap="round"
          strokeDasharray={circumference}
          strokeDashoffset={offset}
          className="transition-[stroke-dashoffset] duration-700 ease-out"
        />
      </svg>
      <div className="absolute inset-0 flex flex-col items-center justify-center">
        <span className="text-3xl font-bold text-foreground">{clamped}%</span>
        {label ? <span className="mt-1 text-xs text-muted-foreground">{label}</span> : null}
      </div>
    </div>
  )
}
