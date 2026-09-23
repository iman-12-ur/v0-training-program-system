using System.ComponentModel.DataAnnotations;

namespace TrainingSystem.Models
{
    // الأنواع الكبرى لفجوة المهارة (ثابتة على مستوى المنظومة)
    public enum SkillGapType
    {
        [Display(Name = "مهارات فنية / تخصصية")]
        Technical = 1,

        [Display(Name = "مهارات سلوكية / إدارية")]
        Behavioral = 2,

        [Display(Name = "مهارات إلزامية / تشريعية")]
        Compliance = 3
    }

    public static class SkillGapTypeExtensions
    {
        public static string DisplayName(this SkillGapType type) => type switch
        {
            SkillGapType.Technical => "مهارات فنية / تخصصية",
            SkillGapType.Behavioral => "مهارات سلوكية / إدارية",
            SkillGapType.Compliance => "مهارات إلزامية / تشريعية",
            _ => type.ToString()
        };
    }

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

        // النوع الكبير الذي ينتمي إليه هذا التصنيف (نوع الفجوة)
        [Display(Name = "نوع الفجوة")]
        public SkillGapType? GapType { get; set; }

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
