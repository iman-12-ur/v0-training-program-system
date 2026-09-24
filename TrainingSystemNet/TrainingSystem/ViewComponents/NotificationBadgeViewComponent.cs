using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.Data;
using TrainingSystem.Models;

namespace TrainingSystem.ViewComponents
{
    // شارة عدد الإشعارات غير المقروءة في القائمة الجانبية
    public class NotificationBadgeViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationBadgeViewComponent(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userId = _userManager.GetUserId((System.Security.Claims.ClaimsPrincipal)User);
            if (string.IsNullOrEmpty(userId)) return View(0);

            var count = await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
            return View(count);
        }
    }
}
