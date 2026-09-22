using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.Data;
using TrainingSystem.Models;

namespace TrainingSystem.Controllers
{
    // السماح لجميع الأدوار بالعرض
    [Authorize(Roles = "SuperAdmin,Admin,Supervisor")]
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
            if (batch.EndDate.Date < batch.StartDate.Date)
            {
                ModelState.AddModelError(nameof(Batch.EndDate), "تاريخ الانتهاء يجب أن يكون بعد تاريخ البدء أو مساوياً له");
            }

            if (!await _context.TrainingPrograms.AnyAsync(p => p.Id == batch.TrainingProgramId))
            {
                ModelState.AddModelError(nameof(Batch.TrainingProgramId), "البرنامج التدريبي المحدد غير موجود");
            }

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

            var existingBatch = await _context.Batches.FindAsync(id);
            if (existingBatch == null)
            {
                return NotFound();
            }

            if (batch.EndDate.Date < batch.StartDate.Date)
            {
                ModelState.AddModelError(nameof(Batch.EndDate), "تاريخ الانتهاء يجب أن يكون بعد تاريخ البدء أو مساوياً له");
            }

            var programExists = await _context.TrainingPrograms.AnyAsync(p => p.Id == batch.TrainingProgramId);
            if (!programExists)
            {
                ModelState.AddModelError(nameof(Batch.TrainingProgramId), "البرنامج التدريبي المحدد غير موجود");
            }

            var approvedCount = await _context.Registrations.CountAsync(r =>
                r.BatchId == id && r.Status == RegistrationStatus.Approved);
            if (batch.MaxParticipants < approvedCount)
            {
                ModelState.AddModelError(nameof(Batch.MaxParticipants),
                    $"لا يمكن خفض السعة عن عدد المقبولين الحالي ({approvedCount})");
            }

            if (ModelState.IsValid)
            {
                existingBatch.Name = batch.Name.Trim();
                existingBatch.StartDate = batch.StartDate;
                existingBatch.EndDate = batch.EndDate;
                existingBatch.MaxParticipants = batch.MaxParticipants;
                existingBatch.Status = batch.Status;
                existingBatch.TrainingProgramId = batch.TrainingProgramId;
                existingBatch.CurrentParticipants = approvedCount;

                await _context.SaveChangesAsync();
                TempData["Success"] = "تم تحديث الدفعة بنجاح";
                return RedirectToAction(nameof(Index));
            }

            batch.CurrentParticipants = existingBatch.CurrentParticipants;
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
                var hasRegistrations = await _context.Registrations.AnyAsync(r => r.BatchId == id);
                if (hasRegistrations)
                {
                    TempData["Error"] = "لا يمكن حذف دفعة مرتبطة بطلبات تسجيل. يمكنك تغيير حالتها إلى ملغاة بدلاً من ذلك.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Batches.Remove(batch);
                await _context.SaveChangesAsync();
                TempData["Success"] = "تم حذف الدفعة بنجاح";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
