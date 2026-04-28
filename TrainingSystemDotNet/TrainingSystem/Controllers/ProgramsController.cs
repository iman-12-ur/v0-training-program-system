using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.Data;
using TrainingSystem.Models;

namespace TrainingSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ProgramsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProgramsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var programs = await _context.TrainingPrograms
                .Include(p => p.Batches)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
            return View(programs);
        }

        public IActionResult Create()
        {
            return View(new TrainingProgram());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TrainingProgram program)
        {
            if (ModelState.IsValid)
            {
                program.CreatedAt = DateTime.Now;
                program.Status = ProgramStatus.Active;
                _context.TrainingPrograms.Add(program);
                await _context.SaveChangesAsync();
                TempData["Success"] = "تم إضافة البرنامج بنجاح";
                return RedirectToAction(nameof(Index));
            }
            return View(program);
        }

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
                    if (!await _context.TrainingPrograms.AnyAsync(e => e.Id == id))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(program);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var program = await _context.TrainingPrograms.FindAsync(id);
            if (program != null)
            {
                program.Status = program.Status == ProgramStatus.Active ? ProgramStatus.Inactive : ProgramStatus.Active;
                await _context.SaveChangesAsync();
                TempData["Success"] = "تم تغيير حالة البرنامج بنجاح";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
