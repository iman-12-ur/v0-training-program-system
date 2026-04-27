using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrainingSystem.Models
{
    public class Batch
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم الدفعة مطلوب")]
        [Display(Name = "اسم الدفعة")]
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
        [Range(1, 1000, ErrorMessage = "يجب أن يكون العدد بين 1 و 1000")]
        public int MaxParticipants { get; set; }

        [Display(Name = "عدد المشاركين الحالي")]
        public int CurrentParticipants { get; set; } = 0;

        [Display(Name = "الحالة")]
        public BatchStatus Status { get; set; } = BatchStatus.Upcoming;

        // Foreign Key
        [Display(Name = "البرنامج التدريبي")]
        public int TrainingProgramId { get; set; }

        // Navigation Property
        [ForeignKey("TrainingProgramId")]
        public virtual TrainingProgram? TrainingProgram { get; set; }

        public virtual ICollection<Registration> Registrations { get; set; } = new List<Registration>();
    }

    public enum BatchStatus
    {
        [Display(Name = "قادمة")]
        Upcoming,
        [Display(Name = "جارية")]
        Ongoing,
        [Display(Name = "مكتملة")]
        Completed,
        [Display(Name = "ملغاة")]
        Cancelled
    }
}
