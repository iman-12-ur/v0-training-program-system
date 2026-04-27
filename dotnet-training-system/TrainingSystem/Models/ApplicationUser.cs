using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace TrainingSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [Display(Name = "الاسم الكامل")]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "الصورة الشخصية")]
        public string? AvatarUrl { get; set; }

        [Display(Name = "الدور")]
        public UserRole Role { get; set; } = UserRole.Admin;

        [Display(Name = "نشط")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "تاريخ الإنشاء")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "آخر تسجيل دخول")]
        public DateTime? LastLoginAt { get; set; }
    }

    public enum UserRole
    {
        [Display(Name = "مدير النظام")]
        SuperAdmin,
        [Display(Name = "مدير")]
        Admin,
        [Display(Name = "مشرف")]
        Supervisor
    }
}
