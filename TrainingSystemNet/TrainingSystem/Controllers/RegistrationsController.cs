using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.Data;
using TrainingSystem.Models;

namespace TrainingSystem.Controllers
{
    // السماح لجميع الأدوار بالعرض
    [Authorize(Roles = "SuperAdmin,Admin,Supervisor")]
    public class RegistrationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public RegistrationsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // عرض الطلبات - متاح للجميع
        public async Task<IActionResult> Index(string? status = null, string? search = null)
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
                query = query.Where(r => 
                    r.VisitorName.Contains(search) || 
                    r.EmployeeId.Contains(search) ||
                    r.Email.Contains(search));
            }

            var registrations = await query
                .OrderByDescending(r => r.RegisteredAt)
                .ToListAsync();

            ViewBag.CurrentStatus = status;
            ViewBag.Search = search;

            return View(registrations);
        }

        // تفاصيل الطلب - متاح للجميع
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

        // قبول الطلب - SuperAdmin و Admin و Supervisor
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Admin,Supervisor")]
        public async Task<IActionResult> Approve(int id)
        {
            var registration = await _context.Registrations.FindAsync(id);
            if (registration != null)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                
                registration.Status = RegistrationStatus.Approved;
                registration.ApprovedBy = currentUser?.FullName ?? "المدير";
                registration.ApprovedAt = DateTime.Now;

                await _context.SaveChangesAsync();
                TempData["Success"] = "تم قبول الطلب بنجاح";
            }

            return RedirectToAction(nameof(Index));
        }

        // رفض الطلب - SuperAdmin و Admin و Supervisor
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Admin,Supervisor")]
        public async Task<IActionResult> Reject(int id, string? notes = null)
        {
            var registration = await _context.Registrations
                .Include(r => r.Batch)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (registration != null)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                
                registration.Status = RegistrationStatus.Rejected;
                registration.ApprovedBy = currentUser?.FullName ?? "المدير";
                registration.ApprovedAt = DateTime.Now;
                registration.Notes = notes;

                // Decrease batch participants count
                if (registration.Batch != null && registration.Batch.CurrentParticipants > 0)
                {
                    registration.Batch.CurrentParticipants--;
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "تم رفض الطلب";
            }

            return RedirectToAction(nameof(Index));
        }

        // حذف الطلب - SuperAdmin و Admin فقط
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var registration = await _context.Registrations
                .Include(r => r.Batch)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (registration != null)
            {
                // Decrease batch participants count
                if (registration.Batch != null && registration.Batch.CurrentParticipants > 0)
                {
                    registration.Batch.CurrentParticipants--;
                }

                _context.Registrations.Remove(registration);
                await _context.SaveChangesAsync();
                TempData["Success"] = "تم حذف الطلب";
            }

            return RedirectToAction(nameof(Index));
        }

        // تصدير التقارير - متاح للجميع
        public async Task<IActionResult> Export()
        {
            var registrations = await _context.Registrations
                .Include(r => r.Batch)
                .ThenInclude(b => b!.TrainingProgram)
                .OrderByDescending(r => r.RegisteredAt)
                .ToListAsync();

            // Generate CSV
            var csv = "الاسم,الرقم الوظيفي,المسمى الوظيفي,الدائرة,القسم,البريد,الهاتف,البرنامج,الدفعة,الحالة,تاريخ التسجيل\n";
            foreach (var r in registrations)
            {
                csv += $"{r.VisitorName},{r.EmployeeId},{r.JobTitle},{r.Court},{r.Department},{r.Email},{r.Phone},{r.Batch?.TrainingProgram?.Title},{r.Batch?.Name},{r.Status},{r.RegisteredAt:yyyy-MM-dd}\n";
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
            return File(bytes, "text/csv", $"registrations_{DateTime.Now:yyyyMMdd}.csv");
        }
    }
}
