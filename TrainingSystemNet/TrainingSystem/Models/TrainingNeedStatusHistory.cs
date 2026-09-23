using System.ComponentModel.DataAnnotations;

namespace TrainingSystem.Models
{
    // سجل تتبّع مراحل اعتماد الاحتياج التدريبي (من قدّم/اعتمد/رفض ومتى)
    public class TrainingNeedStatusHistory
    {
        public int Id { get; set; }

        [Required]
        public int TrainingNeedId { get; set; }
        public TrainingNeed? TrainingNeed { get; set; }

        [Display(Name = "الحالة")]
        public TrainingNeedApprovalStatus Status { get; set; }

        [Display(Name = "الإجراء")]
        [MaxLength(150)]
        public string Action { get; set; } = string.Empty;

        [Display(Name = "التعليق")]
        [MaxLength(500)]
        public string? Comment { get; set; }

        [MaxLength(450)]
        public string? ActionByUserId { get; set; }

        [Display(Name = "اسم المُنفّذ")]
        [MaxLength(150)]
        public string? ActionByName { get; set; }

        public DateTime ActionAt { get; set; } = DateTime.Now;
    }
}
