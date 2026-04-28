using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.Data;
using TrainingSystem.Models;
using TrainingSystem.ViewModels;

namespace TrainingSystem.Controllers
{
    [Authorize(Roles = "Admin")]
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
            var settings = await _context.SystemSettings.FirstOrDefaultAsync() ?? new SystemSettings();
            var user = await _userManager.GetUserAsync(User);

            var viewModel = new DashboardViewModel
            {
                TotalPrograms = await _context.TrainingPrograms.CountAsync(),
                ActivePrograms = await _context.TrainingPrograms.CountAsync(p => p.Status == ProgramStatus.Active),
                TotalRegistrations = await _context.Registrations.CountAsync(),
                PendingRegistrations = await _context.Registrations.CountAsync(r => r.Status == RegistrationStatus.Pending),
                ApprovedRegistrations = await _context.Registrations.CountAsync(r => r.Status == RegistrationStatus.Approved),
                TotalBatches = await _context.Batches.CountAsync(),
                RecentPrograms = await _context.TrainingPrograms
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(5)
                    .ToListAsync(),
                RecentRegistrations = await _context.Registrations
                    .Include(r => r.Batch)
                    .ThenInclude(b => b!.TrainingProgram)
                    .OrderByDescending(r => r.RegisteredAt)
                    .Take(10)
                    .ToListAsync(),
                AdminWelcome = $"{settings.AdminWelcome} {user?.FullName ?? "المدير"}",
                AdminDescription = settings.AdminDescription
            };

            return View(viewModel);
        }
    }
}
