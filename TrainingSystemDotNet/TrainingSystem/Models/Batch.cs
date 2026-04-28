using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrainingSystem.Models
{
    public class Batch
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم الدفعة مطلوب")]
        [Display(Name = "اسم الدفعة")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "تاريخ البداية مطلوب")]
        [Display(Name = "تاريخ البداية")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "تاريخ النهاية مطلوب")]
        [Display(Name = "تاريخ النهاية")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "الحد الأقصى للمشاركين مطلوب")]
        [Display(Name = "الحد الأقصى للمشاركين")]
        [Range(1, 1000)]
        public int MaxParticipants { get; set; }

        [Display(Name = "عدد المشاركين الحالي")]
        public int CurrentParticipants { get; set; } = 0;

        [Display(Name = "الحالة")]
        public BatchStatus Status { get; set; } = BatchStatus.Upcoming;

        [Display(Name = "البرنامج التدريبي")]
        public int TrainingProgramId { get; set; }

        [ForeignKey("TrainingProgramId")]
        public virtual TrainingProgram? TrainingProgram { get; set; }

        // Navigation property
        public virtual ICollection<Registration> Registrations { get; set; } = new List<Registration>();
    }

    public enum BatchStatus
    {
        [Display(Name = "قادمة")]
        Upcoming,
        [Display(Name = "جارية")]
        InProgress,
        [Display(Name = "مكتملة")]
        Completed,
        [Display(Name = "ملغاة")]
        Cancelled
    }
}
