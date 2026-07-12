using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrainingSystem.Models
{
    public class Registration
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "الاسم مطلوب")]
        [Display(Name = "اسم الموظف")]
        public string VisitorName { get; set; } = string.Empty;

        [Required(ErrorMessage = "الرقم الوظيفي مطلوب")]
        [Display(Name = "الرقم الوظيفي")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "الرقم الوظيفي يجب أن يحتوي على أرقام إنجليزية فقط")]
        public string EmployeeId { get; set; } = string.Empty;

        [Required(ErrorMessage = "المسمى الوظيفي مطلوب")]
        [Display(Name = "المسمى الوظيفي")]
        public string JobTitle { get; set; } = string.Empty;

        [Required(ErrorMessage = "الدائرة/المحكمة مطلوبة")]
        [Display(Name = "الدائرة/المحكمة")]
        public string Court { get; set; } = string.Empty;

        [Required(ErrorMessage = "القسم مطلوب")]
        [Display(Name = "القسم")]
        public string Department { get; set; } = string.Empty;

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح")]
        [Display(Name = "البريد الإلكتروني")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        [Display(Name = "رقم الهاتف")]
        [RegularExpression(@"^(968)?[279]\d{7}$", ErrorMessage = "رقم الهاتف يجب أن يبدأ بـ 2 أو 7 أو 9 ويتكون من 8 أرقام (سلطنة عُمان)")]
        public string Phone { get; set; } = string.Empty;

        [Display(Name = "حالة الطلب")]
        public RegistrationStatus Status { get; set; } = RegistrationStatus.Pending;

        [Display(Name = "تاريخ التسجيل")]
        public DateTime RegisteredAt { get; set; } = DateTime.Now;

        [Display(Name = "معتمد من")]
        public string? ApprovedBy { get; set; }

        [Display(Name = "تاريخ الاعتماد")]
        public DateTime? ApprovedAt { get; set; }

        [Display(Name = "ملاحظات")]
        public string? Notes { get; set; }

        // Foreign keys
        [Display(Name = "الدفعة")]
        public int BatchId { get; set; }

        [ForeignKey("BatchId")]
        public virtual Batch? Batch { get; set; }

        // Computed properties for display
        [NotMapped]
        public string ProgramTitle => Batch?.TrainingProgram?.Title ?? "";

        [NotMapped]
        public string BatchName => Batch?.Name ?? "";
    }

    public enum RegistrationStatus
    {
        [Display(Name = "قيد المراجعة")]
        Pending,
        [Display(Name = "مقبول")]
        Approved,
        [Display(Name = "مرفوض")]
        Rejected,
        [Display(Name = "مكتمل")]
        Completed
    }
}
