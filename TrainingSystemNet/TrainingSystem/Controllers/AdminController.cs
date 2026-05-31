using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.Data;
using TrainingSystem.Models;

namespace TrainingSystem.Controllers
{
    // السماح لجميع الأدوار بالدخول للوحة التحكم
    [Authorize(Roles = "SuperAdmin,Admin,Supervisor,Viewer")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var settings = await _context.SystemSettings.FirstOrDefaultAsync();

            ViewBag.AdminName = currentUser?.FullName ?? "المدير";
            ViewBag.AdminWelcome = settings?.AdminWelcome ?? "مرحباً،";
            ViewBag.AdminDescription = settings?.AdminDescription ?? "لوحة إدارة البرامج التدريبية";

            ViewBag.TotalPrograms = await _context.TrainingPrograms.CountAsync();
            ViewBag.TotalBatches = await _context.Batches.CountAsync();
            ViewBag.TotalRegistrations = await _context.Registrations.CountAsync();
            ViewBag.PendingRegistrations = await _context.Registrations.CountAsync(r => r.Status == RegistrationStatus.Pending);

            var recentRegistrations = await _context.Registrations
                .Include(r => r.Batch)
                .ThenInclude(b => b!.TrainingProgram)
                .OrderByDescending(r => r.RegisteredAt)
                .Take(5)
                .ToListAsync();

            return View(recentRegistrations);
        }
    }
}
