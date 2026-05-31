using System.ComponentModel.DataAnnotations;

namespace TrainingSystem.Models
{
    public class SystemSettings
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "عنوان الترحيب")]
        [StringLength(200)]
        public string WelcomeTitle { get; set; } = "مرحباً بك في بوابة التدريب";

        [Display(Name = "وصف الترحيب")]
        public string WelcomeDescription { get; set; } = "استعرض البرامج التدريبية المتاحة وقدّم طلب ترشيحك للبرنامج المناسب.";

        [Display(Name = "ترحيب المدير")]
        [StringLength(100)]
        public string AdminWelcome { get; set; } = "مرحباً،";

        [Display(Name = "وصف لوحة التحكم")]
        public string AdminDescription { get; set; } = "لوحة إدارة البرامج التدريبية - يمكنك إدارة البرامج والتسجيلات من هنا";

        [Display(Name = "اسم المنظمة")]
        [StringLength(200)]
        public string OrganizationName { get; set; } = "المجلس الأعلى للقضاء";

        [Display(Name = "شعار المنظمة")]
        public string? OrganizationLogo { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
