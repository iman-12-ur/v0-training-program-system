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

        // بادئات ثابتة لتذكيرات مواعيد تقييم الأثر (تُستخدم لمنع التكرار)
        public const string PostTrainingDuePrefix = "حان موعد تعبئة التقييم المباشر لتدريب:";
        public const string Day90DuePrefix = "حان موعد تقييم الأثر بعد 90 يوماً لتدريب:";

        // فحص كسول: ينشئ تذكيرات لمواعيد تقييم الأثر المستحقّة التي لم يُذكَّر بها بعد.
        // يُستدعى عند فتح مركز الإشعارات، فلا حاجة لمهمة مجدولة. يستدعي SaveChanges داخلياً.
        public static async Task EnsureAssessmentDueRemindersAsync(ApplicationDbContext context)
        {
            var today = DateTime.Now.Date;

            var dueAssessments = await context.TrainingImpactAssessments
                .Include(a => a.TrainingNeed).ThenInclude(n => n!.Employee)
                .Where(a => a.CompletedAt == null
                            && a.DueDate != null
                            && a.DueDate.Value.Date <= today)
                .ToListAsync();

            if (dueAssessments.Count == 0) return;

            // الإشعارات القائمة المرتبطة بهذه الطلبات (لتفادي التكرار)
            var needIds = dueAssessments.Select(a => a.TrainingNeedId).Distinct().ToList();
            var existing = await context.Notifications
                .Where(n => n.RelatedNeedId != null && needIds.Contains(n.RelatedNeedId.Value))
                .Select(n => new { n.RelatedNeedId, n.Message })
                .ToListAsync();

            bool added = false;
            foreach (var a in dueAssessments)
            {
                var need = a.TrainingNeed;
                if (need == null) continue;

                var prefix = a.AssessmentType == ImpactAssessmentType.Day90
                    ? Day90DuePrefix : PostTrainingDuePrefix;

                // موجود مسبقاً؟
                bool alreadyReminded = existing.Any(e =>
                    e.RelatedNeedId == a.TrainingNeedId &&
                    e.Message != null && e.Message.StartsWith(prefix));
                if (alreadyReminded) continue;

                var recipient = need.Employee?.ManagerUserId ?? need.CreatedByUserId;
                if (string.IsNullOrEmpty(recipient)) continue;

                Add(context, recipient, $"{prefix} {need.SkillName}",
                    NotificationType.Warning, need.Id);
                added = true;
            }

            if (added)
                await context.SaveChangesAsync();
        }
    }
}
