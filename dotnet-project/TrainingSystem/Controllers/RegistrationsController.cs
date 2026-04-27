using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.Data;
using TrainingSystem.Models;

namespace TrainingSystem.Controllers
{
    public class RegistrationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RegistrationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Registrations (Admin only)
        [Authorize(Roles = "Admin,Supervisor")]
        public async Task<IActionResult> Index(string? status, int? programId, string? search)
        {
            var query = _context.Registrations
                .Include(r => r.Batch)
                .ThenInclude(b => b!.TrainingProgram)
                .AsQueryable();

            // Filter by status
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<RegistrationStatus>(status, out var statusEnum))
            {
                query = query.Where(r => r.Status == statusEnum);
            }

            // Filter by program
            if (programId.HasValue)
            {
                query = query.Where(r => r.Batch!.TrainingProgramId == programId);
            }

            // Search
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(r => 
                    r.VisitorName.Contains(search) || 
                    r.EmployeeId.Contains(search) ||
                    r.Email.Contains(search));
            }

            var registrations = await query
                .OrderByDescending(r => r.RegisteredAt)
                .ToListAsync();

            ViewBag.Programs = await _context.TrainingPrograms.ToListAsync();
            ViewBag.CurrentStatus = status;
            ViewBag.CurrentProgramId = programId;
            ViewBag.Search = search;

            return View(registrations);
        }

        // GET: Registrations/Create (Public registration)
        [AllowAnonymous]
        public async Task<IActionResult> Create(int? batchId)
        {
            var batches = await _context.Batches
                .Include(b => b.TrainingProgram)
                .Where(b => b.Status == BatchStatus.Upcoming && 
                           b.CurrentParticipants < b.MaxParticipants &&
                           b.TrainingProgram!.Status == ProgramStatus.Active)
                .ToListAsync();

            ViewBag.Batches = new SelectList(batches, "Id", "Name", batchId);
            ViewBag.SelectedBatchId = batchId;

            return View();
        }

        // POST: Registrations/Create
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Registration registration)
        {
            // Check for duplicate registration
            var existingRegistration = await _context.Registrations
                .FirstOrDefaultAsync(r => r.EmployeeId == registration.EmployeeId && 
                                         r.BatchId == registration.BatchId);

            if (existingRegistration != null)
            {
                ModelState.AddModelError("EmployeeId", "تم تسجيلك مسبقاً في هذه الدفعة");
            }

            // Check batch availability
            var batch = await _context.Batches.FindAsync(registration.BatchId);
            if (batch == null)
            {
                ModelState.AddModelError("BatchId", "الدفعة المحددة غير موجودة");
            }
            else if (batch.CurrentParticipants >= batch.MaxParticipants)
            {
                ModelState.AddModelError("BatchId", "الدفعة مكتملة العدد");
            }

            if (ModelState.IsValid)
            {
                registration.RegisteredAt = DateTime.Now;
                registration.Status = RegistrationStatus.Pending;

                _context.Add(registration);
                await _context.SaveChangesAsync();

                TempData["Success"] = "تم تقديم طلب الترشيح بنجاح. سيتم مراجعته وإبلاغك بالنتيجة.";
                return RedirectToAction("Index", "Home");
            }

            var batches = await _context.Batches
                .Include(b => b.TrainingProgram)
                .Where(b => b.Status == BatchStatus.Upcoming && 
                           b.CurrentParticipants < b.MaxParticipants)
                .ToListAsync();

            ViewBag.Batches = new SelectList(batches, "Id", "Name", registration.BatchId);
            return View(registration);
        }

        // POST: Registrations/Approve/5
        [HttpPost]
        [Authorize(Roles = "Admin,Supervisor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var registration = await _context.Registrations
                .Include(r => r.Batch)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (registration != null && registration.Status == RegistrationStatus.Pending)
            {
                registration.Status = RegistrationStatus.Approved;
                registration.ApprovedAt = DateTime.Now;
                registration.ApprovedBy = User.Identity?.Name;

                // Update batch participant count
                if (registration.Batch != null)
                {
                    registration.Batch.CurrentParticipants++;
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "تم قبول طلب الترشيح";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Registrations/Reject/5
        [HttpPost]
        [Authorize(Roles = "Admin,Supervisor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string? reason)
        {
            var registration = await _context.Registrations.FindAsync(id);

            if (registration != null && registration.Status == RegistrationStatus.Pending)
            {
                registration.Status = RegistrationStatus.Rejected;
                registration.RejectionReason = reason;
                registration.ApprovedAt = DateTime.Now;
                registration.ApprovedBy = User.Identity?.Name;

                await _context.SaveChangesAsync();
                TempData["Success"] = "تم رفض طلب الترشيح";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Registrations/Delete/5
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var registration = await _context.Registrations
                .Include(r => r.Batch)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (registration != null)
            {
                // Update batch participant count if was approved
                if (registration.Status == RegistrationStatus.Approved && registration.Batch != null)
                {
                    registration.Batch.CurrentParticipants--;
                }

                _context.Registrations.Remove(registration);
                await _context.SaveChangesAsync();
                TempData["Success"] = "تم حذف طلب الترشيح";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
