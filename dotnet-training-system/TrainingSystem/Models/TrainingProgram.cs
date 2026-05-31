using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrainingSystem.Models
{
    public class TrainingProgram
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "عنوان البرنامج مطلوب")]
        [Display(Name = "عنوان البرنامج")]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "وصف البرنامج مطلوب")]
        [Display(Name = "وصف البرنامج")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "التصنيفات")]
        public string? Categories { get; set; }

        [Display(Name = "نوع البرنامج")]
        [StringLength(100)]
        public string? ProgramType { get; set; }

        [Display(Name = "الفئة المستهدفة")]
        [StringLength(200)]
        public string? TargetAudience { get; set; }

        [Required(ErrorMessage = "مدة البرنامج مطلوبة")]
        [Display(Name = "المدة")]
        [StringLength(50)]
        public string Duration { get; set; } = string.Empty;

        [Required(ErrorMessage = "اسم المدرب مطلوب")]
        [Display(Name = "المدرب")]
        [StringLength(100)]
        public string Instructor { get; set; } = string.Empty;

        [Required(ErrorMessage = "الموقع مطلوب")]
        [Display(Name = "الموقع")]
        [StringLength(200)]
        public string Location { get; set; } = string.Empty;

        [Display(Name = "صورة البرنامج")]
        public string? ImageUrl { get; set; }

        [Display(Name = "شعار البرنامج")]
        public string? LogoUrl { get; set; }

        [Display(Name = "أهداف البرنامج")]
        public string? Objectives { get; set; }

        [Display(Name = "محاور البرنامج")]
        public string? Topics { get; set; }

        [Display(Name = "المتطلبات المسبقة")]
        public string? Prerequisites { get; set; }

        [Display(Name = "الحالة")]
        public ProgramStatus Status { get; set; } = ProgramStatus.Active;

        [Display(Name = "تاريخ الإنشاء")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "تاريخ التحديث")]
        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        public virtual ICollection<Batch> Batches { get; set; } = new List<Batch>();

        // Helper methods
        [NotMapped]
        public List<string> CategoriesList
        {
            get => string.IsNullOrEmpty(Categories) 
                ? new List<string>() 
                : Categories.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(c => c.Trim()).ToList();
            set => Categories = string.Join(",", value);
        }

        [NotMapped]
        public List<string> ObjectivesList
        {
            get => string.IsNullOrEmpty(Objectives) 
                ? new List<string>() 
                : Objectives.Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(o => o.Trim()).ToList();
            set => Objectives = string.Join("\n", value);
        }

        [NotMapped]
        public List<string> TopicsList
        {
            get => string.IsNullOrEmpty(Topics) 
                ? new List<string>() 
                : Topics.Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim()).ToList();
            set => Topics = string.Join("\n", value);
        }

        [NotMapped]
        public List<string> PrerequisitesList
        {
            get => string.IsNullOrEmpty(Prerequisites) 
                ? new List<string>() 
                : Prerequisites.Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToList();
            set => Prerequisites = string.Join("\n", value);
        }
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
