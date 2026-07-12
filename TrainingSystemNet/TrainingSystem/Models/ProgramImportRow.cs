namespace TrainingSystem.Models
{
    // صف استيراد برنامج من ملف Excel (يُستخدم للمعاينة قبل الحفظ)
    public class ProgramImportRow
    {
        public int RowNumber { get; set; }

        public string? ProgramCode { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Categories { get; set; }
        public string? ProgramType { get; set; }
        public string? TargetAudience { get; set; }
        public string? Duration { get; set; }
        public string? Instructor { get; set; }
        public string? Location { get; set; }
        public string? Objectives { get; set; }
        public string? Topics { get; set; }
        public string? Prerequisites { get; set; }

        // نتيجة التحقق
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}
