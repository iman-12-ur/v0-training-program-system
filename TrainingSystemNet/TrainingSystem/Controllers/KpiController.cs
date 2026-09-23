using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.Data;
using TrainingSystem.Models;

namespace TrainingSystem.Controllers
{
    // محرك مؤشرات الأداء (KPI) لوحدة الاحتياجات التدريبية
    [Authorize(Roles = "SuperAdmin,Admin,Supervisor")]
    public class KpiController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public KpiController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
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

            // 1) تغطية الاحتياجات %
            vm.CoveragePercent = vm.TotalNeeds > 0
                ? (int)Math.Round(100.0 * vm.ApprovedNeeds / vm.TotalNeeds) : 0;

            // 2) الالتزام بالموازنة المركزية (Planned/Actual/Variance/Utilization)
            // الموازنة مركزية لدائرة التدريب — لا تُفلتَر حسب الدائرة الطالبة.
            var budgets = await _context.TrainingBudgets.ToListAsync();
            vm.PlannedBudget = budgets.Sum(b => b.AllocatedBudget);
            vm.ActualSpending = budgets.Sum(b => b.ActualSpending);
            vm.CommittedBudget = budgets.Sum(b => b.CommittedBudget);
            vm.BudgetVariance = vm.PlannedBudget - (vm.ActualSpending + vm.CommittedBudget);
            vm.BudgetUtilizationPercent = vm.PlannedBudget > 0
                ? (int)Math.Round(100m * (vm.ActualSpending + vm.CommittedBudget) / vm.PlannedBudget) : 0;

            // 3) تحسّن الأداء % (من تقييمات الأثر المكتملة)
            var assessmentsQuery = _context.TrainingImpactAssessments
                .Include(a => a.TrainingNeed)
                .Where(a => a.CompletedAt != null);
            if (!IsPrivileged)
                assessmentsQuery = assessmentsQuery.Where(a => a.TrainingNeed != null && a.TrainingNeed.Department == myDept);
            else if (!string.IsNullOrWhiteSpace(department))
                assessmentsQuery = assessmentsQuery.Where(a => a.TrainingNeed != null && a.TrainingNeed.Department == department);

            var assessments = await assessmentsQuery.ToListAsync();
            vm.CompletedAssessments = assessments.Count;
            vm.AveragePerformanceImprovement = assessments.Any()
                ? (int)Math.Round(assessments.Average(a => a.ImprovementPercent)) : 0;

            // 4) ROI % (اختياري): (قيمة التحسّن التقديرية − التكلفة) / التكلفة
            // نُقدّر قيمة التحسّن بنسبة التحسّن مضروبة في التكلفة كبديل تقريبي.
            var completedCost = needs
                .Where(n => n.ApprovalStatus == TrainingNeedApprovalStatus.TrainingCompleted)
                .Sum(n => n.EstimatedCost);
            vm.TotalTrainingCost = completedCost;
            if (completedCost > 0 && assessments.Any())
            {
                var avgImprovementRatio = assessments.Average(a => a.ImprovementPercent) / 100.0;
                var estimatedValue = (double)completedCost * (1 + avgImprovementRatio);
                vm.RoiPercent = (int)Math.Round(100.0 * (estimatedValue - (double)completedCost) / (double)completedCost);
            }

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

        public decimal TotalTrainingCost { get; set; }
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
