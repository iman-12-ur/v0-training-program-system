using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.Data;
using TrainingSystem.Models;

namespace TrainingSystem.Controllers
{
    [Authorize]
    [Route("Admin/[controller]")]
    public class BatchesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BatchesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // قائمة الدفعات
        [HttpGet]
        public async Task<IActionResult> Index(int? programId)
        {
            var query = _context.Batches
                .Include(b => b.TrainingProgram)
                .Include(b => b.Registrations)
                .AsQueryable();

            if (programId.HasValue)
            {
                query = query.Where(b => b.TrainingProgramId == programId.Value);
            }

            var batches = await query.OrderByDescending(b => b.StartDate).ToListAsync();
            ViewBag.Programs = await _context.TrainingPrograms.ToListAsync();
            ViewBag.SelectedProgram = programId;

            return View(batches);
        }

        // إضافة دفعة - عرض النموذج
        [HttpGet("Create")]
        public async Task<IActionResult> Create(int? programId)
        {
            ViewBag.Programs = new SelectList(
                await _context.TrainingPrograms.ToListAsync(),
                "Id",
                "Title",
                programId
            );

            var batch = new Batch();
            if (programId.HasValue)
            {
                batch.TrainingProgramId = programId.Value;
            }

            return View(batch);
        }

        // إضافة دفعة - حفظ
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Batch batch)
        {
            if (ModelState.IsValid)
            {
                batch.CreatedAt = DateTime.Now;
                batch.Status = BatchStatus.Upcoming;

                _context.Batches.Add(batch);
                await _context.SaveChangesAsync();

                TempData["Success"] = "تم إضافة الدفعة بنجاح";
                return RedirectToAction(nameof(Index), new { programId = batch.TrainingProgramId });
            }

            ViewBag.Programs = new SelectList(
                await _context.TrainingPrograms.ToListAsync(),
                "Id",
                "Title",
                batch.TrainingProgramId
            );

            return View(batch);
        }

        // تعديل دفعة - عرض النموذج
        [HttpGet("Edit/{id}")]
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
                batch.TrainingProgramId
            );

            return View(batch);
        }

        // تعديل دفعة - حفظ
        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
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
                    var existingBatch = await _context.Batches.FindAsync(id);
                    if (existingBatch == null)
                    {
                        return NotFound();
                    }

                    existingBatch.Name = batch.Name;
                    existingBatch.StartDate = batch.StartDate;
                    existingBatch.EndDate = batch.EndDate;
                    existingBatch.MaxParticipants = batch.MaxParticipants;
                    existingBatch.Status = batch.Status;
                    existingBatch.TrainingProgramId = batch.TrainingProgramId;

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

                return RedirectToAction(nameof(Index), new { programId = batch.TrainingProgramId });
            }

            ViewBag.Programs = new SelectList(
                await _context.TrainingPrograms.ToListAsync(),
                "Id",
                "Title",
                batch.TrainingProgramId
            );

            return View(batch);
        }

        // حذف دفعة
        [HttpPost("Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var batch = await _context.Batches.FindAsync(id);
            if (batch == null)
            {
                return NotFound();
            }

            var programId = batch.TrainingProgramId;
            _context.Batches.Remove(batch);
            await _context.SaveChangesAsync();

            TempData["Success"] = "تم حذف الدفعة بنجاح";
            return RedirectToAction(nameof(Index), new { programId });
        }
    }
}
