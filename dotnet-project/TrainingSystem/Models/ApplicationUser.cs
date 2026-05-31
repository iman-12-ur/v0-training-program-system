using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace TrainingSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [Display(Name = "الاسم الكامل")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "الدور")]
        public UserRole Role { get; set; } = UserRole.Viewer;

        [Display(Name = "نشط")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "تاريخ الإنشاء")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public enum UserRole
    {
        [Display(Name = "مدير النظام")]
        Admin,
        [Display(Name = "مشرف")]
        Supervisor,
        [Display(Name = "عارض")]
        Viewer
    }
}
