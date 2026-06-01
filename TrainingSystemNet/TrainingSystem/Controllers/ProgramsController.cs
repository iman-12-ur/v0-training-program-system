using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.Data;
using TrainingSystem.Models;

namespace TrainingSystem.Controllers
{
    // السماح لجميع الأدوار بالعرض
    [Authorize(Roles = "SuperAdmin,Admin,Supervisor")]
    public class ProgramsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProgramsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // عرض البرامج - متاح للجميع
        public async Task<IActionResult> Index()
        {
            var programs = await _context.TrainingPrograms
                .Include(p => p.Batches)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(programs);
        }

        // إضافة برنامج - SuperAdmin و Admin فقط
        [Authorize(Roles = "SuperAdmin,Admin")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> Create(TrainingProgram program)
        {
            if (ModelState.IsValid)
            {
                program.Status = ProgramStatus.Active;
                program.CreatedAt = DateTime.Now;

                _context.TrainingPrograms.Add(program);
                await _context.SaveChangesAsync();

                TempData["Success"] = "تم إضافة البرنامج بنجاح";
                return RedirectToAction(nameof(Index));
            }

            return View(program);
        }

        // تعديل برنامج - SuperAdmin و Admin فقط
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var program = await _context.TrainingPrograms.FindAsync(id);
            if (program == null)
            {
                return NotFound();
            }

            return View(program);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> Edit(int id, TrainingProgram program)
        {
            if (id != program.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(program);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "تم تحديث البرنامج بنجاح";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.TrainingPrograms.AnyAsync(p => p.Id == id))
                    {
                        return NotFound();
                    }
                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(program);
        }

        // حذف برنامج - SuperAdmin و Admin فقط
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var program = await _context.TrainingPrograms.FindAsync(id);
            if (program != null)
            {
                _context.TrainingPrograms.Remove(program);
                await _context.SaveChangesAsync();
                TempData["Success"] = "تم حذف البرنامج بنجاح";
            }

            return RedirectToAction(nameof(Index));
        }

        // تغيير حالة البرنامج - SuperAdmin و Admin فقط
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var program = await _context.TrainingPrograms.FindAsync(id);
            if (program != null)
            {
                program.Status = program.Status == ProgramStatus.Active 
                    ? ProgramStatus.Inactive 
                    : ProgramStatus.Active;
                
                await _context.SaveChangesAsync();
                TempData["Success"] = program.Status == ProgramStatus.Active 
                    ? "تم تفعيل البرنامج" 
                    : "تم إيقاف البرنامج";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
