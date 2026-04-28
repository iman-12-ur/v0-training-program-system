using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrainingSystem.Models
{
    public class Registration
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "الاسم مطلوب")]
        [Display(Name = "الاسم")]
        [StringLength(100)]
        public string VisitorName { get; set; } = string.Empty;

        [Required(ErrorMessage = "الرقم الوظيفي مطلوب")]
        [Display(Name = "الرقم الوظيفي")]
        [StringLength(20)]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "الرقم الوظيفي يجب أن يحتوي على أرقام إنجليزية فقط")]
        public string EmployeeId { get; set; } = string.Empty;

        [Required(ErrorMessage = "الدائرة/المحكمة مطلوبة")]
        [Display(Name = "الدائرة/المحكمة")]
        [StringLength(100)]
        public string Court { get; set; } = string.Empty;

        [Required(ErrorMessage = "القسم مطلوب")]
        [Display(Name = "القسم")]
        [StringLength(100)]
        public string Department { get; set; } = string.Empty;

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [Display(Name = "البريد الإلكتروني")]
        [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح")]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "رقم الجوال مطلوب")]
        [Display(Name = "رقم الجوال")]
        [StringLength(20)]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "رقم الجوال يجب أن يحتوي على أرقام إنجليزية فقط")]
        public string Phone { get; set; } = string.Empty;

        [Display(Name = "الحالة")]
        public RegistrationStatus Status { get; set; } = RegistrationStatus.Pending;

        [Display(Name = "تاريخ التسجيل")]
        public DateTime RegisteredAt { get; set; } = DateTime.Now;

        [Display(Name = "تمت الموافقة بواسطة")]
        public string? ApprovedBy { get; set; }

        [Display(Name = "تاريخ الموافقة")]
        public DateTime? ApprovedAt { get; set; }

        [Display(Name = "الدفعة")]
        public int BatchId { get; set; }

        [ForeignKey("BatchId")]
        public virtual Batch? Batch { get; set; }
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
