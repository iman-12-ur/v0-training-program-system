using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.Data;
using TrainingSystem.Models;

namespace TrainingSystem.Controllers
{
    // السماح لجميع الأدوار بالعرض
    [Authorize(Roles = "SuperAdmin,Admin,Supervisor,Viewer")]
    public class BatchesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BatchesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // عرض الدفعات - متاح للجميع
        public async Task<IActionResult> Index()
        {
            var batches = await _context.Batches
                .Include(b => b.TrainingProgram)
                .Include(b => b.Registrations)
                .OrderByDescending(b => b.StartDate)
                .ToListAsync();

            return View(batches);
        }

        // إضافة دفعة - SuperAdmin و Admin فقط
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> Create(int? programId = null)
        {
            ViewBag.Programs = new SelectList(
                await _context.TrainingPrograms.ToListAsync(), 
                "Id", 
                "Title", 
                programId);

            return View(new Batch { TrainingProgramId = programId ?? 0 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> Create(Batch batch)
        {
            if (ModelState.IsValid)
            {
                batch.Status = BatchStatus.Upcoming;
                batch.CurrentParticipants = 0;

                _context.Batches.Add(batch);
                await _context.SaveChangesAsync();

                TempData["Success"] = "تم إضافة الدفعة بنجاح";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Programs = new SelectList(
                await _context.TrainingPrograms.ToListAsync(), 
                "Id", 
                "Title", 
                batch.TrainingProgramId);

            return View(batch);
        }

        // تعديل دفعة - SuperAdmin و Admin فقط
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var batch = await _context.Batches.FindAsync(id);
            if (batch == null)
            {
                return NotFound();
            }

            ViewBag.Programs = new SelectList(
                await _context.TrainingPrograms.ToListAsync(), 
                "Id", 
                "Title", 
                batch.TrainingProgramId);

            return View(batch);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> Edit(int id, Batch batch)
        {
            if (id != batch.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(batch);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "تم تحديث الدفعة بنجاح";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Batches.AnyAsync(b => b.Id == id))
                    {
                        return NotFound();
                    }
                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewBag.Programs = new SelectList(
                await _context.TrainingPrograms.ToListAsync(), 
                "Id", 
                "Title", 
                batch.TrainingProgramId);

            return View(batch);
        }

        // حذف دفعة - SuperAdmin و Admin فقط
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var batch = await _context.Batches.FindAsync(id);
            if (batch != null)
            {
                _context.Batches.Remove(batch);
                await _context.SaveChangesAsync();
                TempData["Success"] = "تم حذف الدفعة بنجاح";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
