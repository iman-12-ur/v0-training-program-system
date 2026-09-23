using System.ComponentModel.DataAnnotations;

namespace TrainingSystem.Models
{
    // تصنيف المهارات (فني، سلوكي، قيادي، ...)
    public class SkillCategory
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "اسم التصنيف")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "الوصف")]
        [MaxLength(300)]
        public string? Description { get; set; }

        [Display(Name = "نشط")]
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public List<Skill> Skills { get; set; } = new();
    }

    // مهارة قابلة لإعادة الاستخدام ضمن تصنيف
    public class Skill
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "اسم المهارة")]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "التصنيف")]
        public int? SkillCategoryId { get; set; }
        public SkillCategory? SkillCategory { get; set; }

        [Display(Name = "الوصف")]
        [MaxLength(300)]
        public string? Description { get; set; }

        [Display(Name = "نشط")]
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
