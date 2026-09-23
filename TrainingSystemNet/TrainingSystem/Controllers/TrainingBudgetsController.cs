using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.Data;
using TrainingSystem.Models;

namespace TrainingSystem.Controllers
{
    // إدارة الموازنة المركزية لدائرة التدريب حسب السنة المالية (دائرة التدريب فقط).
    // للمتابعة فقط — لا توقف اعتماد الاحتياجات.
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

            var budget = await _context.TrainingBudgets
                .FirstOrDefaultAsync(b => b.FinancialYear == selected);

            ViewBag.Years = years;
            ViewBag.SelectedYear = selected;
            return View(budget);
        }

        public IActionResult Create()
        {
            return View(new TrainingBudget { FinancialYear = CurrentFinancialYear() });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TrainingBudget model)
        {
            if (string.IsNullOrWhiteSpace(model.FinancialYear))
                ModelState.AddModelError(nameof(model.FinancialYear), "السنة المالية مطلوبة");
            if (model.AllocatedBudget < 0)
                ModelState.AddModelError(nameof(model.AllocatedBudget), "الميزانية لا يمكن أن تكون سالبة");

            var exists = await _context.TrainingBudgets.AnyAsync(b => b.FinancialYear == model.FinancialYear);
            if (exists)
                ModelState.AddModelError(string.Empty, "توجد موازنة معرّفة لهذه السنة المالية");

            if (!ModelState.IsValid)
                return View(model);

            model.FinancialYear = model.FinancialYear.Trim();
            model.CreatedAt = DateTime.Now;
            _context.TrainingBudgets.Add(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = "تم إضافة الموازنة المركزية";
            return RedirectToAction(nameof(Index), new { financialYear = model.FinancialYear });
        }

        public async Task<IActionResult> Edit(int id)
        {
            var budget = await _context.TrainingBudgets.FindAsync(id);
            if (budget == null) return NotFound();
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
                return View(model);

            // السنة ثابتة (مفتاح فريد)؛ يُعدَّل المبلغ المخصص فقط يدوياً
            budget.AllocatedBudget = model.AllocatedBudget;
            budget.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["Success"] = "تم تحديث الميزانية المخصصة";
            return RedirectToAction(nameof(Index), new { financialYear = budget.FinancialYear });
        }

        private static string CurrentFinancialYear()
        {
            var y = DateTime.Now.Year;
            return $"{y}/{y + 1}";
        }
    }
}
