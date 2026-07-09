using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrainingSystem.Models
{
    public class TrainingProgram
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "عنوان البرنامج مطلوب")]
        [Display(Name = "عنوان البرنامج")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "وصف البرنامج مطلوب")]
        [Display(Name = "وصف البرنامج")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "التصنيفات")]
        public string? Categories { get; set; }

        [Display(Name = "نوع البرنامج")]
        public string? ProgramType { get; set; }

        [Display(Name = "الفئة المستهدفة")]
        public string? TargetAudience { get; set; }

        [Required(ErrorMessage = "المدة مطلوبة")]
        [Display(Name = "المدة")]
        public string Duration { get; set; } = string.Empty;

        [Required(ErrorMessage = "اسم المدرب مطلوب")]
        [Display(Name = "المدرب")]
        public string Instructor { get; set; } = string.Empty;

        [Required(ErrorMessage = "الموقع مطلوب")]
        [Display(Name = "الموقع")]
        public string Location { get; set; } = string.Empty;

        [Display(Name = "شعار البرنامج")]
        public string? Logo { get; set; }

        [Display(Name = "أهداف البرنامج")]
        public string? Objectives { get; set; }

        [Display(Name = "محاور البرنامج")]
        public string? Topics { get; set; }

        [Display(Name = "المتطلبات المسبقة")]
        public string? Prerequisites { get; set; }

        [Display(Name = "بداية فترة الترشيح")]
        [DataType(DataType.Date)]
        public DateTime? RegistrationStartDate { get; set; }

        [Display(Name = "نهاية فترة الترشيح")]
        [DataType(DataType.Date)]
        public DateTime? RegistrationEndDate { get; set; }

        [Display(Name = "حالة البرنامج")]
        public ProgramStatus Status { get; set; } = ProgramStatus.Active;

        [Display(Name = "تاريخ الإنشاء")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // هل باب الترشيح مفتوح حالياً؟
        [NotMapped]
        public bool IsRegistrationOpen
        {
            get
            {
                var today = DateTime.Today;
                if (RegistrationStartDate.HasValue && today < RegistrationStartDate.Value.Date) return false;
                if (RegistrationEndDate.HasValue && today > RegistrationEndDate.Value.Date) return false;
                return true;
            }
        }

        // Navigation property
        public virtual ICollection<Batch> Batches { get; set; } = new List<Batch>();
    }

    public enum ProgramStatus
    {
        [Display(Name = "نشط")]
        Active,
        [Display(Name = "غير نشط")]
        Inactive,
        [Display(Name = "مسودة")]
        Draft
    }
}
