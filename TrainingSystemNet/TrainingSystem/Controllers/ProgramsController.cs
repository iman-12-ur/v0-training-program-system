using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.Data;
using TrainingSystem.Models;

namespace TrainingSystem.Controllers
{
    // السماح لجميع الأدوار بالعرض
    [Authorize(Roles = "SuperAdmin,Admin,Supervisor")]
    public class ProgramsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProgramsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // عرض البرامج - متاح للجميع
        public async Task<IActionResult> Index()
        {
            var programs = await _context.TrainingPrograms
                .Include(p => p.Batches)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(programs);
        }

        // إضافة برنامج - SuperAdmin و Admin فقط
        [Authorize(Roles = "SuperAdmin,Admin")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> Create(TrainingProgram program)
        {
            if (ModelState.IsValid)
            {
                program.Status = ProgramStatus.Active;
                program.CreatedAt = DateTime.Now;

                _context.TrainingPrograms.Add(program);
                await _context.SaveChangesAsync();

                TempData["Success"] = "تم إضافة البرنامج بنجاح";
                return RedirectToAction(nameof(Index));
            }

            return View(program);
        }

        // تعديل برنامج - SuperAdmin و Admin فقط
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var program = await _context.TrainingPrograms.FindAsync(id);
            if (program == null)
            {
                return NotFound();
            }

            return View(program);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> Edit(int id, TrainingProgram program)
        {
            if (id != program.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(program);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "تم تحديث البرنامج بنجاح";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.TrainingPrograms.AnyAsync(p => p.Id == id))
                    {
                        return NotFound();
                    }
                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(program);
        }

        // حذف برنامج - SuperAdmin و Admin فقط
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var program = await _context.TrainingPrograms.FindAsync(id);
            if (program != null)
            {
                _context.TrainingPrograms.Remove(program);
                await _context.SaveChangesAsync();
                TempData["Success"] = "تم حذف البرنامج بنجاح";
            }

            return RedirectToAction(nameof(Index));
        }

        // تغيير حالة البرنامج - SuperAdmin و Admin فقط
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var program = await _context.TrainingPrograms.FindAsync(id);
            if (program != null)
            {
                program.Status = program.Status == ProgramStatus.Active 
                    ? ProgramStatus.Inactive 
                    : ProgramStatus.Active;
                
                await _context.SaveChangesAsync();
                TempData["Success"] = program.Status == ProgramStatus.Active 
                    ? "تم تفعيل البرنامج" 
                    : "تم إيقاف البرنامج";
            }

            return RedirectToAction(nameof(Index));
        }

        // ===== استيراد البرامج عبر ملف Excel =====

        // ترتيب أعمدة القالب (نفس توزيعة النظام)
        private static readonly string[] TemplateHeaders = new[]
        {
            "عنوان البرنامج",
            "وصف البرنامج",
            "التصنيفات",
            "نوع البرنامج",
            "الفئة المستهدفة",
            "المدة",
            "المدرب",
            "الموقع",
            "الأهداف",
            "المحاور",
            "المتطلبات المسبقة",
            "بداية فترة الترشيح",
            "نهاية فترة الترشيح"
        };

        // تحميل قالب Excel فارغ - SuperAdmin و Admin فقط
        [Authorize(Roles = "SuperAdmin,Admin")]
        public IActionResult DownloadTemplate()
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("البرامج");
            ws.RightToLeft = true;

            // رأس الأعمدة
            for (int i = 0; i < TemplateHeaders.Length; i++)
            {
                var cell = ws.Cell(1, i + 1);
                cell.Value = TemplateHeaders[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1e88e5");
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            // صف مثال توضيحي
            var example = new[]
            {
                "القيادة الفعّالة",
                "برنامج تدريبي لتطوير المهارات القيادية",
                "إدارية، قيادية",
                "حضوري",
                "المشرفين والمدراء",
                "5 أيام",
                "أ. محمد العامري",
                "قاعة التدريب الرئيسية",
                "فهم أساسيات القيادة | تطوير مهارات التواصل",
                "أنماط القيادة | إدارة الفرق | حل المشكلات",
                "خبرة سنتين | موافقة المدير المباشر",
                DateTime.Today.ToString("yyyy-MM-dd"),
                DateTime.Today.AddDays(14).ToString("yyyy-MM-dd")
            };
            for (int i = 0; i < example.Length; i++)
            {
                ws.Cell(2, i + 1).Value = example[i];
            }

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();

            return File(
                content,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "قالب_البرامج_التدريبية.xlsx");
        }

        // رفع ملف Excel واستيراد البرامج - SuperAdmin و Admin فقط
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> Import(IFormFile? file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "الرجاء اختيار ملف Excel صالح";
                return RedirectToAction(nameof(Index));
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (extension != ".xlsx" && extension != ".xls")
            {
                TempData["Error"] = "صيغة الملف غير مدعومة. الرجاء رفع ملف بصيغة .xlsx";
                return RedirectToAction(nameof(Index));
            }

            var imported = new List<TrainingProgram>();
            var errors = new List<string>();

            try
            {
                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                stream.Position = 0;

                using var workbook = new XLWorkbook(stream);
                var ws = workbook.Worksheet(1);
                var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;

                // نبدأ من الصف الثاني (بعد رأس الأعمدة)
                for (int row = 2; row <= lastRow; row++)
                {
                    string Get(int col) => ws.Cell(row, col).GetString().Trim();

                    var title = Get(1);
                    var description = Get(2);
                    var duration = Get(6);
                    var instructor = Get(7);
                    var location = Get(8);

                    // تجاهل الصفوف الفارغة تماماً
                    if (string.IsNullOrWhiteSpace(title) &&
                        string.IsNullOrWhiteSpace(description) &&
                        string.IsNullOrWhiteSpace(instructor))
                    {
                        continue;
                    }

                    // التحقق من الحقول المطلوبة
                    if (string.IsNullOrWhiteSpace(title) ||
                        string.IsNullOrWhiteSpace(description) ||
                        string.IsNullOrWhiteSpace(duration) ||
                        string.IsNullOrWhiteSpace(instructor) ||
                        string.IsNullOrWhiteSpace(location))
                    {
                        errors.Add($"الصف {row}: نقص في الحقول المطلوبة (العنوان/الوصف/المدة/المدرب/الموقع)");
                        continue;
                    }

                    var program = new TrainingProgram
                    {
                        Title = title,
                        Description = description,
                        Categories = NormalizeList(Get(3)),
                        ProgramType = string.IsNullOrWhiteSpace(Get(4)) ? null : Get(4),
                        TargetAudience = string.IsNullOrWhiteSpace(Get(5)) ? null : Get(5),
                        Duration = duration,
                        Instructor = instructor,
                        Location = location,
                        Objectives = NormalizeList(Get(9)),
                        Topics = NormalizeList(Get(10)),
                        Prerequisites = NormalizeList(Get(11)),
                        RegistrationStartDate = ParseDate(Get(12)),
                        RegistrationEndDate = ParseDate(Get(13)),
                        Status = ProgramStatus.Active,
                        CreatedAt = DateTime.Now
                    };

                    imported.Add(program);
                }

                if (imported.Count > 0)
                {
                    _context.TrainingPrograms.AddRange(imported);
                    await _context.SaveChangesAsync();
                }

                if (imported.Count > 0 && errors.Count == 0)
                {
                    TempData["Success"] = $"تم استيراد {imported.Count} برنامج تدريبي بنجاح";
                }
                else if (imported.Count > 0 && errors.Count > 0)
                {
                    TempData["Success"] = $"تم استيراد {imported.Count} برنامج، مع تجاهل {errors.Count} صف: {string.Join(" - ", errors)}";
                }
                else
                {
                    TempData["Error"] = errors.Count > 0
                        ? $"لم يتم استيراد أي برنامج. {string.Join(" - ", errors)}"
                        : "لم يتم العثور على بيانات صالحة في الملف";
                }
            }
            catch (Exception)
            {
                TempData["Error"] = "تعذّر قراءة الملف. تأكد أنه ملف Excel صالح بنفس القالب المطلوب";
            }

            return RedirectToAction(nameof(Index));
        }

        // توحيد قوائم العناصر (تصنيفات/أهداف/محاور) إلى سطور مفصولة
        private static string? NormalizeList(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            var items = value
                .Split(new[] { '|', '\n', '،', ',', '؛', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => s.Length > 0);
            return string.Join("\n", items);
        }

        // تحويل نص التاريخ إلى DateTime إن أمكن
        private static DateTime? ParseDate(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            if (DateTime.TryParse(value, out var date)) return date;
            return null;
        }
    }
}
