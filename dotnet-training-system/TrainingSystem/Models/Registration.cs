using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrainingSystem.Models
{
    public class Registration
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم الموظف مطلوب")]
        [Display(Name = "اسم الموظف")]
        [StringLength(100)]
        public string EmployeeName { get; set; } = string.Empty;

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
        [RegularExpression(@"^[0-9+]+$", ErrorMessage = "رقم الجوال يجب أن يحتوي على أرقام إنجليزية فقط")]
        public string Phone { get; set; } = string.Empty;

        [Display(Name = "الحالة")]
        public RegistrationStatus Status { get; set; } = RegistrationStatus.Pending;

        [Display(Name = "تاريخ التسجيل")]
        public DateTime RegisteredAt { get; set; } = DateTime.Now;

        [Display(Name = "تمت الموافقة بواسطة")]
        [StringLength(100)]
        public string? ApprovedBy { get; set; }

        [Display(Name = "تاريخ الموافقة")]
        public DateTime? ApprovedAt { get; set; }

        [Display(Name = "سبب الرفض")]
        public string? RejectionReason { get; set; }

        // Foreign Keys
        [Required]
        public int BatchId { get; set; }

        [Required]
        public int TrainingProgramId { get; set; }

        // Navigation Properties
        [ForeignKey("BatchId")]
        public virtual Batch? Batch { get; set; }

        [ForeignKey("TrainingProgramId")]
        public virtual TrainingProgram? TrainingProgram { get; set; }

        // Computed Properties
        [NotMapped]
        public string StatusDisplay => Status switch
        {
            RegistrationStatus.Pending => "قيد المراجعة",
            RegistrationStatus.Approved => "مقبول",
            RegistrationStatus.Rejected => "مرفوض",
            RegistrationStatus.Completed => "مكتمل",
            _ => "غير معروف"
        };

        [NotMapped]
        public string StatusBadgeClass => Status switch
        {
            RegistrationStatus.Pending => "bg-warning",
            RegistrationStatus.Approved => "bg-success",
            RegistrationStatus.Rejected => "bg-danger",
            RegistrationStatus.Completed => "bg-info",
            _ => "bg-secondary"
        };
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
