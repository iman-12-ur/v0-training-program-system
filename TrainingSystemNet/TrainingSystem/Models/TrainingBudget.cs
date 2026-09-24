using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrainingSystem.Models
{
    // موازنة مركزية واحدة لدائرة التدريب ضمن سنة مالية.
    // تُدار بالكامل من قِبَل دائرة التدريب — للمتابعة فقط دون إيقاف الاعتمادات.
    public class TrainingBudget
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "السنة المالية")]
        [MaxLength(9)]
        public string FinancialYear { get; set; } = string.Empty;

        [Display(Name = "الميزانية المخصصة")]
        [Range(0, double.MaxValue)]
        public decimal AllocatedBudget { get; set; }

        // المبلغ المرتبط بطلبات معتمدة (التزام)
        [Display(Name = "المبلغ الملتزم به")]
        [Range(0, double.MaxValue)]
        public decimal CommittedBudget { get; set; }

        // المصروف الفعلي بعد تنفيذ التدريب
        [Display(Name = "المصروف الفعلي")]
        [Range(0, double.MaxValue)]
        public decimal ActualSpending { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        // المتبقي = المخصص - (الملتزم + الفعلي). قد يكون سالباً (متابعة فقط).
        [NotMapped]
        public decimal RemainingBudget => AllocatedBudget - (CommittedBudget + ActualSpending);

        // نسبة الاستهلاك %
        [NotMapped]
        public int UtilizationPercent => AllocatedBudget > 0
            ? (int)Math.Round(100m * (CommittedBudget + ActualSpending) / AllocatedBudget)
            : 0;
    }
}
