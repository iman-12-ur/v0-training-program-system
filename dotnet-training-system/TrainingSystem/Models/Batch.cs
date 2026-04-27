using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrainingSystem.Models
{
    public class Batch
    {
        [Key]
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
        [Range(1, 1000, ErrorMessage = "يجب أن يكون العدد بين 1 و 1000")]
        public int MaxParticipants { get; set; }

        [Display(Name = "عدد المشاركين الحالي")]
        public int CurrentParticipants { get; set; } = 0;

        [Display(Name = "الحالة")]
        public BatchStatus Status { get; set; } = BatchStatus.Upcoming;

        [Display(Name = "تاريخ الإنشاء")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Foreign Key
        [Required]
        public int TrainingProgramId { get; set; }

        // Navigation Properties
        [ForeignKey("TrainingProgramId")]
        public virtual TrainingProgram? TrainingProgram { get; set; }

        public virtual ICollection<Registration> Registrations { get; set; } = new List<Registration>();

        // Computed Properties
        [NotMapped]
        public bool IsFull => CurrentParticipants >= MaxParticipants;

        [NotMapped]
        public int AvailableSeats => MaxParticipants - CurrentParticipants;

        [NotMapped]
        public string StatusDisplay => Status switch
        {
            BatchStatus.Upcoming => "قادم",
            BatchStatus.InProgress => "جاري",
            BatchStatus.Completed => "مكتمل",
            BatchStatus.Cancelled => "ملغي",
            _ => "غير معروف"
        };
    }

    public enum BatchStatus
    {
        [Display(Name = "قادم")]
        Upcoming,
        [Display(Name = "جاري")]
        InProgress,
        [Display(Name = "مكتمل")]
        Completed,
        [Display(Name = "ملغي")]
        Cancelled
    }
}
