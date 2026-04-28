using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.Data;
using TrainingSystem.Models;

namespace TrainingSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RegistrationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public RegistrationsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string? status, string? search, int? programId)
        {
            var query = _context.Registrations
                .Include(r => r.Batch)
                .ThenInclude(b => b!.TrainingProgram)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<RegistrationStatus>(status, out var statusEnum))
            {
                query = query.Where(r => r.Status == statusEnum);
            }

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(r => r.VisitorName.Contains(search) || 
                                          r.EmployeeId.Contains(search) ||
                                          r.Email.Contains(search));
            }

            if (programId.HasValue)
            {
                query = query.Where(r => r.Batch!.TrainingProgramId == programId);
            }

            var registrations = await query.OrderByDescending(r => r.RegisteredAt).ToListAsync();

            ViewBag.Programs = await _context.TrainingPrograms.ToListAsync();
            ViewBag.CurrentStatus = status;
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentProgramId = programId;

            return View(registrations);
        }

        public async Task<IActionResult> Details(int id)
        {
            var registration = await _context.Registrations
                .Include(r => r.Batch)
                .ThenInclude(b => b!.TrainingProgram)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (registration == null)
            {
                return NotFound();
            }

            return View(registration);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var registration = await _context.Registrations.FindAsync(id);
            if (registration != null)
            {
                var user = await _userManager.GetUserAsync(User);
                registration.Status = RegistrationStatus.Approved;
                registration.ApprovedBy = user?.FullName ?? "المدير";
                registration.ApprovedAt = DateTime.Now;
                await _context.SaveChangesAsync();
                TempData["Success"] = "تم قبول طلب الترشيح";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var registration = await _context.Registrations
                .Include(r => r.Batch)
                .FirstOrDefaultAsync(r => r.Id == id);
                
            if (registration != null)
            {
                var user = await _userManager.GetUserAsync(User);
                registration.Status = RegistrationStatus.Rejected;
                registration.ApprovedBy = user?.FullName ?? "المدير";
                registration.ApprovedAt = DateTime.Now;

                if (registration.Batch != null && registration.Batch.CurrentParticipants > 0)
                {
                    registration.Batch.CurrentParticipants--;
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "تم رفض طلب الترشيح";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var registration = await _context.Registrations
                .Include(r => r.Batch)
                .FirstOrDefaultAsync(r => r.Id == id);
                
            if (registration != null)
            {
                if (registration.Batch != null && registration.Batch.CurrentParticipants > 0)
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
