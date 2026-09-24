using System.ComponentModel.DataAnnotations;

namespace TrainingSystem.Models
{
    // نوع الإشعار (يحدد الأيقونة واللون في الواجهة)
    public enum NotificationType
    {
        Info = 0,
        Success = 1,
        Warning = 2,
        Danger = 3
    }

    // إشعار موجّه لمستخدم بخصوص حدث في سير عمل الاحتياجات
    public class Notification
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(450)]
        public string UserId { get; set; } = string.Empty;

        [Display(Name = "النوع")]
        public NotificationType Type { get; set; } = NotificationType.Info;

        [Required]
        [Display(Name = "الرسالة")]
        [MaxLength(500)]
        public string Message { get; set; } = string.Empty;

        // ربط اختياري بالاحتياج ذي الصلة (للانتقال إليه)
        public int? RelatedNeedId { get; set; }

        [Display(Name = "مقروء")]
        public bool IsRead { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public static string GetIcon(NotificationType t) => t switch
        {
            NotificationType.Success => "bi-check-circle",
            NotificationType.Warning => "bi-exclamation-triangle",
            NotificationType.Danger => "bi-x-circle",
            _ => "bi-info-circle"
        };

        public static string GetColor(NotificationType t) => t switch
        {
            NotificationType.Success => "success",
            NotificationType.Warning => "warning",
            NotificationType.Danger => "danger",
            _ => "info"
        };
    }
}
