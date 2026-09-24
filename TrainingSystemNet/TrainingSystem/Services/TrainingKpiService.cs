using Microsoft.EntityFrameworkCore;
using TrainingSystem.Data;
using TrainingSystem.Models;

namespace TrainingSystem.Services
{
    // الخدمة المركزية لحساب مؤشرات الأداء (KPI) لوحدة الاحتياجات التدريبية.
    // كل معادلات المؤشرات تُعرَّف هنا مرة واحدة، وتستدعيها كل الصفحات (Dashboard, KPI, Reports)
    // بدل تكرار المعادلات داخل كل صفحة.
    public class TrainingKpiService
    {
        private readonly ApplicationDbContext _context;

        public TrainingKpiService(ApplicationDbContext context)
        {
            _context = context;
        }

        // مفتاح تمييز الموظف (رقم وظيفي + الاسم) لعدّ الموظفين المميّزين لا الطلبات.
        private static string EmpKey(TrainingNeed n) => (n.EmployeeNumber ?? "") + "|" + n.EmployeeName;

        // الحالات التي تُعتبر «أُكمل تدريب الموظف فيها».
        public static readonly TrainingNeedApprovalStatus[] CompletedStatuses =
        {
            TrainingNeedApprovalStatus.TrainingCompleted,
            TrainingNeedApprovalStatus.ImpactAssessmentPending,
            TrainingNeedApprovalStatus.ImpactAssessmentCompleted,
            TrainingNeedApprovalStatus.Closed
        };

        // يحسب المؤشرات الأربعة من مجموعة الاحتياجات المفلترة + معطيات الميزانية + عائد ROI اختياري.
        // نطاق الصلاحية/الفلترة مسؤولية المستدعي؛ الخدمة تحسب المعادلات فقط.
        public async Task<KpiEngine> ComputeAsync(
            IReadOnlyList<TrainingNeed> needs,
            decimal budgetPlanned = 0m,
            decimal budgetActual = 0m,
            decimal budgetCommitted = 0m,
            bool hasBudget = false,
            decimal roiTrainingCost = 0m,
            decimal? roiReturn = null)
        {
            // KPI 1 — نسبة تغطية الاحتياجات التدريبية
            // = (عدد الموظفين الذين أُكمل تدريبهم ÷ عدد الموظفين المرصود لهم احتياج) × 100
            var trainedEmployees = needs
                .Where(n => CompletedStatuses.Contains(n.ApprovalStatus))
                .Select(EmpKey).Distinct().Count();
            var targetedEmployees = needs.Select(EmpKey).Distinct().Count();

            // KPI 3 — معدل تحسن الأداء بعد التدريب (من تقييمات الأثر المكتملة)
            var needIds = needs.Select(n => n.Id).ToList();
            var assessments = await _context.TrainingImpactAssessments
                .Where(a => needIds.Contains(a.TrainingNeedId) && a.CompletedAt != null)
                .Select(a => new { a.TrainingNeedId, a.BeforeScore, a.AfterScore })
                .ToListAsync();

            var needToEmp = needs.ToDictionary(n => n.Id, EmpKey);
            var assessedEmployees = assessments
                .Select(a => needToEmp[a.TrainingNeedId]).Distinct().Count();
            var improvedEmployees = assessments
                .Where(a => a.AfterScore > a.BeforeScore)
                .Select(a => needToEmp[a.TrainingNeedId]).Distinct().Count();

            return new KpiEngine
            {
                TrainedEmployees = trainedEmployees,
                TargetedEmployees = targetedEmployees,

                HasBudget = hasBudget,
                BudgetPlanned = budgetPlanned,
                BudgetActual = budgetActual,
                BudgetCommitted = budgetCommitted,

                AssessedEmployees = assessedEmployees,
                ImprovedEmployees = improvedEmployees,

                RoiAvailable = roiReturn.HasValue && roiReturn.Value > 0,
                RoiFinancialReturn = roiReturn ?? 0m,
                RoiTrainingCost = roiTrainingCost
            };
        }
    }

    // نتيجة محرك المؤشرات — كل النسب مشتقّة من الأرقام الخام، فالمعادلة في مكان واحد.
    public class KpiEngine
    {
        // KPI 1 — نسبة تغطية الاحتياجات التدريبية
        public int TrainedEmployees { get; set; }
        public int TargetedEmployees { get; set; }
        public int CoveragePercent => TargetedEmployees > 0
            ? (int)Math.Round(100.0 * TrainedEmployees / TargetedEmployees) : 0;

        // KPI 2 — الالتزام بالميزانية
        public bool HasBudget { get; set; }
        public decimal BudgetPlanned { get; set; }
        public decimal BudgetActual { get; set; }
        public decimal BudgetCommitted { get; set; }
        public decimal BudgetUsed => BudgetActual + BudgetCommitted;
        public decimal BudgetVariance => BudgetPlanned - BudgetUsed;
        public int BudgetUtilizationPercent => BudgetPlanned > 0
            ? (int)Math.Round(100m * BudgetUsed / BudgetPlanned) : 0;

        // KPI 3 — معدل تحسن الأداء بعد التدريب
        public int ImprovedEmployees { get; set; }
        public int AssessedEmployees { get; set; }
        public int PerformanceImprovementPercent => AssessedEmployees > 0
            ? (int)Math.Round(100.0 * ImprovedEmployees / AssessedEmployees) : 0;

        // KPI 4 — العائد على الاستثمار (اختياري)
        public bool RoiAvailable { get; set; }
        public decimal RoiFinancialReturn { get; set; }
        public decimal RoiTrainingCost { get; set; }
        public int RoiPercent => RoiAvailable && RoiTrainingCost > 0
            ? (int)Math.Round(100m * (RoiFinancialReturn - RoiTrainingCost) / RoiTrainingCost) : 0;
    }
}
