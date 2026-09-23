using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.Data;
using TrainingSystem.Models;
using TrainingSystem.Services;

namespace TrainingSystem.Controllers
{
    // مركز الإشعارات — كل مستخدم يرى إشعاراته فقط
    [Authorize(Roles = "SuperAdmin,Admin,Supervisor")]
    public class NotificationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationsController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            // توليد تذكيرات مواعيد تقييم الأثر المستحقّة (فحص كسول بلا مهمة مجدولة)
            await NotificationHelper.EnsureAssessmentDueRemindersAsync(_context);

            var userId = _userManager.GetUserId(User);
            var items = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(100)
                .ToListAsync();
            return View(items);
        }

        // فتح إشعار: يعلّمه كمقروء ثم ينتقل للاحتياج المرتبط إن وُجد
        public async Task<IActionResult> Open(int id)
        {
            var userId = _userManager.GetUserId(User);
            var n = await _context.Notifications
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
            if (n == null) return NotFound();

            if (!n.IsRead)
            {
                n.IsRead = true;
                await _context.SaveChangesAsync();
            }

            if (n.RelatedNeedId.HasValue)
                return RedirectToAction("Details", "TrainingNeeds", new { id = n.RelatedNeedId.Value });

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllRead()
        {
            var userId = _userManager.GetUserId(User);
            var unread = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();
            foreach (var n in unread) n.IsRead = true;
            await _context.SaveChangesAsync();
            TempData["Success"] = "تم تعليم جميع الإشعارات كمقروءة";
            return RedirectToAction(nameof(Index));
        }
    }
}
