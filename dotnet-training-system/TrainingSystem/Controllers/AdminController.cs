using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.Data;
using TrainingSystem.Models;
using TrainingSystem.ViewModels;

namespace TrainingSystem.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // لوحة التحكم الرئيسية
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var settings = await _context.SystemSettings.FirstOrDefaultAsync() ?? new SystemSettings();

            var viewModel = new DashboardViewModel
            {
                TotalPrograms = await _context.TrainingPrograms.CountAsync(),
                ActivePrograms = await _context.TrainingPrograms.CountAsync(p => p.Status == ProgramStatus.Active),
                TotalRegistrations = await _context.Registrations.CountAsync(),
                PendingRegistrations = await _context.Registrations.CountAsync(r => r.Status == RegistrationStatus.Pending),
                ApprovedRegistrations = await _context.Registrations.CountAsync(r => r.Status == RegistrationStatus.Approved),
                TotalBatches = await _context.Batches.CountAsync(),
                UpcomingBatches = await _context.Batches.CountAsync(b => b.Status == BatchStatus.Upcoming),
                RecentPrograms = await _context.TrainingPrograms
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(5)
                    .ToListAsync(),
                RecentRegistrations = await _context.Registrations
                    .Include(r => r.TrainingProgram)
                    .Include(r => r.Batch)
                    .OrderByDescending(r => r.RegisteredAt)
                    .Take(10)
                    .ToListAsync(),
                Settings = settings,
                AdminName = user?.FullName ?? "مدير النظام"
            };

            return View(viewModel);
        }
    }
}
