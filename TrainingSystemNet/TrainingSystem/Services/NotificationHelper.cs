using Microsoft.EntityFrameworkCore;
using TrainingSystem.Data;
using TrainingSystem.Models;

namespace TrainingSystem.Services
{
    // مساعد لإنشاء الإشعارات دون الحاجة لخدمة مسجّلة في DI.
    // لا يستدعي SaveChanges — يُترك للمستدعي لدمجه ضمن نفس المعاملة عند الإمكان.
    public static class NotificationHelper
    {
        public static void Add(ApplicationDbContext context, string userId, string message,
            NotificationType type = NotificationType.Info, int? relatedNeedId = null)
        {
            if (string.IsNullOrWhiteSpace(userId)) return;

            context.Notifications.Add(new Notification
            {
                UserId = userId,
                Message = message,
                Type = type,
                RelatedNeedId = relatedNeedId,
                CreatedAt = DateTime.Now
            });
        }

        // إشعار لكل المستخدمين ضمن دور معيّن (مثل دائرة التدريب)
        public static async Task AddToRoleAsync(ApplicationDbContext context,
            IEnumerable<string> userIds, string message,
            NotificationType type = NotificationType.Info, int? relatedNeedId = null)
        {
            foreach (var uid in userIds.Distinct())
                Add(context, uid, message, type, relatedNeedId);
            await Task.CompletedTask;
        }
    }
}
