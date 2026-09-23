using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrainingSystem.Models
{
    // ميزانية تدريب لدائرة معيّنة ضمن سنة مالية
    public class TrainingBudget
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "الدائرة")]
        [MaxLength(100)]
        public string Department { get; set; } = string.Empty;

        [Required]
        [Display(Name = "السنة المالية")]
        [MaxLength(9)]
        public string FinancialYear { get; set; } = string.Empty;

        [Display(Name = "الميزانية المخصصة")]
        [Range(0, double.MaxValue)]
        public decimal AllocatedBudget { get; set; }

        // المبلغ المرتبط بطلبات معتمدة/قيد الاعتماد
        [Display(Name = "المبلغ الملتزم به")]
        [Range(0, double.MaxValue)]
        public decimal CommittedBudget { get; set; }

        // المصروف الفعلي بعد تنفيذ التدريب
        [Display(Name = "المصروف الفعلي")]
        [Range(0, double.MaxValue)]
        public decimal ActualSpending { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        // المتبقي = المخصص - (الملتزم + الفعلي)
        [NotMapped]
        public decimal RemainingBudget => AllocatedBudget - (CommittedBudget + ActualSpending);

        // نسبة الاستهلاك %
        [NotMapped]
        public int UtilizationPercent => AllocatedBudget > 0
            ? (int)Math.Round(100m * (CommittedBudget + ActualSpending) / AllocatedBudget)
            : 0;
    }
}
