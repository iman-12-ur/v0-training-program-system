using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.Data;
using TrainingSystem.Models;

namespace TrainingSystem.Controllers
{
    // إدارة ميزانيات التدريب حسب الدائرة والسنة المالية (الموارد البشرية فقط)
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class TrainingBudgetsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TrainingBudgetsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? financialYear)
        {
            var years = await _context.TrainingBudgets
                .Select(b => b.FinancialYear)
                .Distinct()
                .OrderByDescending(y => y)
                .ToListAsync();

            var current = CurrentFinancialYear();
            if (!years.Contains(current)) years.Insert(0, current);

            var selected = string.IsNullOrWhiteSpace(financialYear) ? current : financialYear;

            var budgets = await _context.TrainingBudgets
                .Where(b => b.FinancialYear == selected)
                .OrderBy(b => b.Department)
                .ToListAsync();

            ViewBag.Years = years;
            ViewBag.SelectedYear = selected;
            return View(budgets);
        }

        public async Task<IActionResult> Create()
        {
            var vm = new TrainingBudget { FinancialYear = CurrentFinancialYear() };
            await PopulateDepartmentsAsync();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TrainingBudget model)
        {
            if (string.IsNullOrWhiteSpace(model.Department))
                ModelState.AddModelError(nameof(model.Department), "الدائرة مطلوبة");
            if (string.IsNullOrWhiteSpace(model.FinancialYear))
                ModelState.AddModelError(nameof(model.FinancialYear), "السنة المالية مطلوبة");
            if (model.AllocatedBudget < 0)
                ModelState.AddModelError(nameof(model.AllocatedBudget), "الميزانية لا يمكن أن تكون سالبة");

            var exists = await _context.TrainingBudgets.AnyAsync(b =>
                b.Department == model.Department && b.FinancialYear == model.FinancialYear);
            if (exists)
                ModelState.AddModelError(string.Empty, "توجد ميزانية معرّفة لهذه الدائرة في نفس السنة المالية");

            if (!ModelState.IsValid)
            {
                await PopulateDepartmentsAsync();
                return View(model);
            }

            model.Department = model.Department.Trim();
            model.FinancialYear = model.FinancialYear.Trim();
            model.CreatedAt = DateTime.Now;
            _context.TrainingBudgets.Add(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = "تم إضافة ميزانية الدائرة";
            return RedirectToAction(nameof(Index), new { financialYear = model.FinancialYear });
        }

        public async Task<IActionResult> Edit(int id)
        {
            var budget = await _context.TrainingBudgets.FindAsync(id);
            if (budget == null) return NotFound();
            await PopulateDepartmentsAsync();
            return View(budget);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TrainingBudget model)
        {
            if (id != model.Id) return NotFound();
            var budget = await _context.TrainingBudgets.FindAsync(id);
            if (budget == null) return NotFound();

            if (model.AllocatedBudget < 0)
                ModelState.AddModelError(nameof(model.AllocatedBudget), "الميزانية لا يمكن أن تكون سالبة");

            if (!ModelState.IsValid)
            {
                await PopulateDepartmentsAsync();
                return View(model);
            }

            // الدائرة والسنة ثابتتان (مفتاح فريد)؛ يُعدَّل المبلغ المخصص فقط يدوياً
            budget.AllocatedBudget = model.AllocatedBudget;
            budget.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["Success"] = "تم تحديث الميزانية المخصصة";
            return RedirectToAction(nameof(Index), new { financialYear = budget.FinancialYear });
        }

        private async Task PopulateDepartmentsAsync()
        {
            ViewBag.Departments = await _context.Users
                .Where(u => u.Department != null && u.Department != "")
                .Select(u => u.Department!)
                .Distinct()
                .OrderBy(d => d)
                .ToListAsync();
        }

        private static string CurrentFinancialYear()
        {
            var y = DateTime.Now.Year;
            return $"{y}/{y + 1}";
        }
    }
}
