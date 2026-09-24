using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.Data;
using TrainingSystem.Models;
using TrainingSystem.Services;

namespace TrainingSystem.Controllers
{
    // محرك مؤشرات الأداء (KPI) لوحدة الاحتياجات التدريبية
    [Authorize(Roles = "SuperAdmin,Admin,Supervisor")]
    public class KpiController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly TrainingKpiService _kpiService;

        public KpiController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, TrainingKpiService kpiService)
        {
            _context = context;
            _userManager = userManager;
            _kpiService = kpiService;
        }

        private bool IsPrivileged =>
            User.IsInRole(SystemRoles.SuperAdmin) || User.IsInRole(SystemRoles.Admin);

        public async Task<IActionResult> Index(string? department)
        {
            var actor = await _userManager.GetUserAsync(User);
            var myDept = actor?.Department;

            var needsQuery = _context.TrainingNeeds.AsQueryable();
            if (!IsPrivileged)
                needsQuery = needsQuery.Where(n => n.Department == myDept);
            else if (!string.IsNullOrWhiteSpace(department))
                needsQuery = needsQuery.Where(n => n.Department == department);

            var needs = await needsQuery.ToListAsync();

            var approvedStatuses = new[]
            {
                TrainingNeedApprovalStatus.HRApproved,
                TrainingNeedApprovalStatus.TrainingScheduled,
                TrainingNeedApprovalStatus.TrainingCompleted
            };

            var vm = new KpiViewModel
            {
                TotalNeeds = needs.Count,
                ApprovedNeeds = needs.Count(n => approvedStatuses.Contains(n.ApprovalStatus)),
                CompletedTraining = needs.Count(n => n.ApprovalStatus == TrainingNeedApprovalStatus.TrainingCompleted),
            };

            // الموازنة مركزية لدائرة التدريب — لا تُفلتَر حسب الدائرة الطالبة.
            var budgets = await _context.TrainingBudgets.ToListAsync();
            vm.PlannedBudget = budgets.Sum(b => b.AllocatedBudget);
            vm.ActualSpending = budgets.Sum(b => b.ActualSpending);
            vm.CommittedBudget = budgets.Sum(b => b.CommittedBudget);

            // تقدير قيمة التحسّن لعائد ROI (بديل تقريبي = التكلفة × (1 + متوسط نسبة التحسّن))
            // يبقى هذا التقدير مدخلاً للخدمة؛ أما معادلة ROI نفسها فتُحسب مركزياً في الخدمة.
            var completedCost = needs
                .Where(n => n.ApprovalStatus == TrainingNeedApprovalStatus.TrainingCompleted)
                .Sum(n => n.EstimatedCost);
            vm.TotalTrainingCost = completedCost;

            decimal? roiReturn = null;
            if (completedCost > 0)
            {
                var needIds = needs.Select(n => n.Id).ToList();
                var avgImprovementPercent = await _context.TrainingImpactAssessments
                    .Where(a => a.CompletedAt != null && needIds.Contains(a.TrainingNeedId))
                    .Select(a => (double?)(a.AfterScore > a.BeforeScore && a.BeforeScore > 0
                        ? 100.0 * (a.AfterScore - a.BeforeScore) / a.BeforeScore : 0))
                    .AverageAsync() ?? 0;
                if (avgImprovementPercent > 0)
                    roiReturn = completedCost * (decimal)(1 + avgImprovementPercent / 100.0);
            }

            // كل معادلات المؤشرات تُحسب مركزياً عبر TrainingKpiService
            var kpi = await _kpiService.ComputeAsync(
                needs,
                budgetPlanned: vm.PlannedBudget,
                budgetActual: vm.ActualSpending,
                budgetCommitted: vm.CommittedBudget,
                hasBudget: vm.PlannedBudget > 0,
                roiTrainingCost: completedCost,
                roiReturn: roiReturn);

            // 1) تغطية الاحتياجات %
            vm.CoveragePercent = kpi.CoveragePercent;
            // 2) الالتزام بالموازنة
            vm.BudgetVariance = kpi.BudgetVariance;
            vm.BudgetUtilizationPercent = kpi.BudgetUtilizationPercent;
            // 3) تحسّن الأداء %
            vm.CompletedAssessments = kpi.AssessedEmployees;
            vm.AveragePerformanceImprovement = kpi.PerformanceImprovementPercent;
            // 4) نسبة إغلاق الاحتياجات
            vm.CompletedNeeds = kpi.CompletedNeeds;
            vm.NeedsClosurePercent = kpi.NeedsClosurePercent;
            // 5) نسبة الاحتياجات الحرجة التي تمت معالجتها
            vm.CriticalNeeds = kpi.CriticalNeeds;
            vm.CriticalCompleted = kpi.CriticalCompleted;
            vm.CriticalAddressedPercent = kpi.CriticalAddressedPercent;
            // 6) نسبة الالتزام بخطة التدريب
            vm.PlannedNeeds = kpi.PlannedNeeds;
            vm.PlannedCompleted = kpi.PlannedCompleted;
            vm.PlanCommitmentPercent = kpi.PlanCommitmentPercent;
            // العائد على الاستثمار % (اختياري)
            vm.RoiAvailable = kpi.RoiAvailable;
            vm.RoiPercent = kpi.RoiPercent;

            // توزيع حسب الدائرة (للمخوّلين)
            vm.ByDepartment = needs
                .GroupBy(n => n.Department)
                .Select(g => new KpiDepartmentRow
                {
                    Department = g.Key,
                    TotalNeeds = g.Count(),
                    ApprovedNeeds = g.Count(n => approvedStatuses.Contains(n.ApprovalStatus)),
                    EstimatedCost = g.Sum(n => n.EstimatedCost)
                })
                .OrderByDescending(r => r.TotalNeeds)
                .ToList();

            ViewBag.Departments = await _context.Users
                .Where(u => u.Department != null && u.Department != "")
                .Select(u => u.Department!).Distinct().OrderBy(d => d).ToListAsync();
            ViewBag.CurrentDepartment = department;
            ViewBag.IsPrivileged = IsPrivileged;

            return View(vm);
        }
    }

    public class KpiViewModel
    {
        public int TotalNeeds { get; set; }
        public int ApprovedNeeds { get; set; }
        public int CompletedTraining { get; set; }
        public int CoveragePercent { get; set; }

        public decimal PlannedBudget { get; set; }
        public decimal CommittedBudget { get; set; }
        public decimal ActualSpending { get; set; }
        public decimal BudgetVariance { get; set; }
        public int BudgetUtilizationPercent { get; set; }

        public int CompletedAssessments { get; set; }
        public int AveragePerformanceImprovement { get; set; }

        // KPI 4 — نسبة إغلاق الاحتياجات
        public int CompletedNeeds { get; set; }
        public int NeedsClosurePercent { get; set; }

        // KPI 5 — نسبة الاحتياجات الحرجة التي تمت معالجتها
        public int CriticalNeeds { get; set; }
        public int CriticalCompleted { get; set; }
        public int CriticalAddressedPercent { get; set; }

        // KPI 6 — نسبة الالتزام بخطة التدريب
        public int PlannedNeeds { get; set; }
        public int PlannedCompleted { get; set; }
        public int PlanCommitmentPercent { get; set; }

        public decimal TotalTrainingCost { get; set; }
        // ROI اختياري: بعض البرامج لا يمكن قياس عائدها المالي مباشرةً
        public bool RoiAvailable { get; set; }
        public int RoiPercent { get; set; }

        public List<KpiDepartmentRow> ByDepartment { get; set; } = new();
    }

    public class KpiDepartmentRow
    {
        public string Department { get; set; } = string.Empty;
        public int TotalNeeds { get; set; }
        public int ApprovedNeeds { get; set; }
        public decimal EstimatedCost { get; set; }
        public int CoveragePercent => TotalNeeds > 0 ? (int)Math.Round(100.0 * ApprovedNeeds / TotalNeeds) : 0;
    }
}
