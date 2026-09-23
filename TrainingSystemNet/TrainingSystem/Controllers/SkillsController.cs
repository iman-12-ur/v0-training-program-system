using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.Data;
using TrainingSystem.Models;

namespace TrainingSystem.Controllers
{
    // إدارة مكتبة المهارات وتصنيفاتها (تُستخدم في نماذج الاحتياجات التدريبية).
    // الإدارة الكاملة للمدير/مدير النظام فقط.
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class SkillsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SkillsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var vm = new SkillsIndexViewModel
            {
                Categories = await _context.SkillCategories
                    .OrderBy(c => c.Name)
                    .ToListAsync(),
                Skills = await _context.Skills
                    .Include(s => s.SkillCategory)
                    .OrderBy(s => s.Name)
                    .ToListAsync()
            };
            return View(vm);
        }

        // ---------- التصنيفات ----------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(string name, string? description)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["Error"] = "اسم التصنيف مطلوب";
                return RedirectToAction(nameof(Index));
            }

            var exists = await _context.SkillCategories
                .AnyAsync(c => c.Name.ToLower() == name.Trim().ToLower());
            if (exists)
            {
                TempData["Error"] = "هذا التصنيف موجود مسبقاً";
                return RedirectToAction(nameof(Index));
            }

            _context.SkillCategories.Add(new SkillCategory
            {
                Name = name.Trim(),
                Description = description?.Trim(),
                IsActive = true,
                CreatedAt = DateTime.Now
            });
            await _context.SaveChangesAsync();
            TempData["Success"] = "تم إضافة التصنيف";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleCategory(int id)
        {
            var cat = await _context.SkillCategories.FindAsync(id);
            if (cat == null) return RedirectToAction(nameof(Index));
            cat.IsActive = !cat.IsActive;
            await _context.SaveChangesAsync();
            TempData["Success"] = cat.IsActive ? "تم تفعيل التصنيف" : "تم تعطيل التصنيف";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var cat = await _context.SkillCategories.FindAsync(id);
            if (cat == null) return RedirectToAction(nameof(Index));

            var inUse = await _context.Skills.AnyAsync(s => s.SkillCategoryId == id);
            if (inUse)
            {
                TempData["Error"] = "لا يمكن حذف تصنيف مرتبط بمهارات. عطّله بدلاً من ذلك";
                return RedirectToAction(nameof(Index));
            }

            _context.SkillCategories.Remove(cat);
            await _context.SaveChangesAsync();
            TempData["Success"] = "تم حذف التصنيف";
            return RedirectToAction(nameof(Index));
        }

        // ---------- المهارات ----------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSkill(string name, int? skillCategoryId, string? description)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["Error"] = "اسم المهارة مطلوب";
                return RedirectToAction(nameof(Index));
            }

            var exists = await _context.Skills
                .AnyAsync(s => s.Name.ToLower() == name.Trim().ToLower());
            if (exists)
            {
                TempData["Error"] = "هذه المهارة موجودة مسبقاً";
                return RedirectToAction(nameof(Index));
            }

            _context.Skills.Add(new Skill
            {
                Name = name.Trim(),
                SkillCategoryId = skillCategoryId,
                Description = description?.Trim(),
                IsActive = true,
                CreatedAt = DateTime.Now
            });
            await _context.SaveChangesAsync();
            TempData["Success"] = "تم إضافة المهارة";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleSkill(int id)
        {
            var skill = await _context.Skills.FindAsync(id);
            if (skill == null) return RedirectToAction(nameof(Index));
            skill.IsActive = !skill.IsActive;
            await _context.SaveChangesAsync();
            TempData["Success"] = skill.IsActive ? "تم تفعيل المهارة" : "تم تعطيل المهارة";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSkill(int id)
        {
            var skill = await _context.Skills.FindAsync(id);
            if (skill == null) return RedirectToAction(nameof(Index));

            var inUse = await _context.TrainingNeeds.AnyAsync(n => n.SkillId == id);
            if (inUse)
            {
                TempData["Error"] = "لا يمكن حذف مهارة مستخدمة في احتياجات تدريبية. عطّلها بدلاً من ذلك";
                return RedirectToAction(nameof(Index));
            }

            _context.Skills.Remove(skill);
            await _context.SaveChangesAsync();
            TempData["Success"] = "تم حذف المهارة";
            return RedirectToAction(nameof(Index));
        }
    }

    public class SkillsIndexViewModel
    {
        public List<SkillCategory> Categories { get; set; } = new();
        public List<Skill> Skills { get; set; } = new();

        public List<SelectListItem> CategoryOptions =>
            Categories.Where(c => c.IsActive)
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .ToList();
    }
}
