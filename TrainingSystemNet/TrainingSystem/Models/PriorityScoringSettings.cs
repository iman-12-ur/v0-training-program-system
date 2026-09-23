namespace TrainingSystem.Models
{
    // إعدادات حساب أولوية الاحتياج التدريبي.
    // الأوزان والحدود مركزية هنا ليسهل مستقبلاً نقلها إلى إعدادات النظام (SystemSettings)
    // دون تعديل منطق الحساب في أماكن متعددة.
    public static class PriorityScoringSettings
    {
        // أوزان العوامل (مجموعها 100)
        public static double GapWeight { get; set; } = 30.0;         // درجة الفجوة
        public static double ImpactWeight { get; set; } = 20.0;      // تأثير الفجوة على العمل
        public static double RiskWeight { get; set; } = 20.0;        // مستوى المخاطر
        public static double ComplianceWeight { get; set; } = 30.0;  // الإلزامية

        // حدود التصنيف (0-100)
        public static int CriticalThreshold { get; set; } = 80;  // 80-100 = حرجة جداً
        public static int HighThreshold { get; set; } = 60;      // 60-79  = عالية
        public static int MediumThreshold { get; set; } = 40;    // 40-59  = متوسطة
        // أقل من MediumThreshold = منخفضة

        // أقصى قيم العوامل الخام (تُستخدم للتطبيع إلى 0-100)
        public const int MaxGap = 3;      // درجة الفجوة تُقصّ عند 3
        public const int MaxImpact = 4;   // 1-4
        public const int MaxRisk = 4;     // 1-4
    }
}
