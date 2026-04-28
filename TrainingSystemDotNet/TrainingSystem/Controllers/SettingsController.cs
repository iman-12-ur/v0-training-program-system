using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.Data;
using TrainingSystem.Models;

namespace TrainingSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SettingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SettingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var settings = await _context.SystemSettings.FirstOrDefaultAsync() ?? new SystemSettings();
            return View(settings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(SystemSettings settings)
        {
            if (ModelState.IsValid)
            {
                var existingSettings = await _context.SystemSettings.FirstOrDefaultAsync();
                if (existingSettings != null)
                {
                    existingSettings.WelcomeTitle = settings.WelcomeTitle;
                    existingSettings.WelcomeDescription = settings.WelcomeDescription;
                    existingSettings.AdminWelcome = settings.AdminWelcome;
                    existingSettings.AdminDescription = settings.AdminDescription;
                    _context.Update(existingSettings);
                }
                else
                {
                    _context.SystemSettings.Add(settings);
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "تم حفظ الإعدادات بنجاح";
            }
            return View(settings);
        }
    }
}
