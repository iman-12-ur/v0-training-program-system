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
                program.ReferenceNumber = await GenerateReferenceNumberAsync();

                _context.TrainingPrograms.Add(program);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"تم إضافة البرنامج بنجاح - الرقم المرجعي: {program.ReferenceNumber}";
                return RedirectToAction(nameof(Index));
            }

            return View(program);
        }

        // توليد رقم مرجعي تلقائي فريد بصيغة PRG-{السنة}-{تسلسل}
        private async Task<string> GenerateReferenceNumberAsync()
        {
            var year = DateTime.Now.Year;
            var prefix = $"PRG-{year}-";

            var lastForYear = await _context.TrainingPrograms
                .Where(p => p.ReferenceNumber != null && p.ReferenceNumber.StartsWith(prefix))
                .OrderByDescending(p => p.ReferenceNumber)
                .Select(p => p.ReferenceNumber)
                .FirstOrDefaultAsync();

            var next = 1;
            if (!string.IsNullOrEmpty(lastForYear))
            {
                var numericPart = lastForYear.Substring(prefix.Length);
                if (int.TryParse(numericPart, out var parsed))
                {
                    next = parsed + 1;
                }
            }

            return $"{prefix}{next:D4}";
        }

        // ==================== استيراد Excel ====================

        // أعمدة القالب (بالترتيب) - تطابق نموذج البرنامج
        private static readonly string[] TemplateHeaders = new[]
        {
            "رمز البرنامج",
            "عنوان البرنامج *",
            "وصف البرنامج *",
            "التصنيفات",
            "نوع البرنامج",
            "الفئة المستهدفة",
            "المدة *",
            "المدرب *",
            "الموقع *",
            "أهداف البرنامج",
            "محاور البرنامج",
            "المتطلبات المسبقة"
        };

        // تحميل قالب Excel مطابق للنموذج
        [Authorize(Roles = "SuperAdmin,Admin")]
        public IActionResult DownloadTemplate()
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("البرامج");
            ws.RightToLeft = true;

            // ترويسة الأعمدة
            for (int i = 0; i < TemplateHeaders.Length; i++)
            {
                var cell = ws.Cell(1, i + 1);
                cell.Value = TemplateHeaders[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1e3a5f");
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            // صف مثال توضيحي
            var example = new[]
            {
                "LEAD-101",
                "القيادة الفعّالة",
                "برنامج متخصص في تطوير المهارات القيادية",
                "القيادة والإدارة، المهارات الشخصية",
                "حضوري",
                "المدراء ورؤساء الأقسام",
                "5 أيام",
                "د. محمد العامري",
                "قاعة التدريب الرئيسية",
                "فهم أساسيات القيادة | تطوير مهارات التواصل",
                "مقدمة في القيادة | إدارة الفرق | حل المشكلات",
                "لا يوجد"
            };
            for (int i = 0; i < example.Length; i++)
            {
                ws.Cell(2, i + 1).Value = example[i];
            }

            ws.Columns().AdjustToContents();
            ws.Column(1).Width = 15;
            for (int i = 2; i <= TemplateHeaders.Length; i++)
            {
                if (ws.Column(i).Width > 40) ws.Column(i).Width = 40;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"program-import-template-{DateTime.Now:yyyyMMdd}.xlsx");
        }

        // معاينة ملف Excel المرفوع قبل الاستيراد
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public IActionResult ImportPreview(IFormFile? file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "الرجاء اختيار ملف Excel صالح";
                return RedirectToAction(nameof(Create));
            }

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (ext != ".xlsx" && ext != ".xls")
            {
                TempData["Error"] = "صيغة الملف غير مدعومة. الرجاء رفع ملف بصيغة .xlsx";
                return RedirectToAction(nameof(Create));
            }

            List<ProgramImportRow> rows;
            try
            {
                rows = ParseExcel(file);
            }
            catch
            {
                TempData["Error"] = "تعذّر قراءة الملف. تأكد من استخدام القالب الصحيح";
                return RedirectToAction(nameof(Create));
            }

            if (rows.Count == 0)
            {
                TempData["Error"] = "الملف لا يحتوي على بيانات برامج";
                return RedirectToAction(nameof(Create));
            }

            return View("ImportPreview", rows);
        }

        // تأكيد الاستيراد وحفظ الصفوف الصالحة
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> ImportConfirm(List<ProgramImportRow> rows)
        {
            if (rows == null || rows.Count == 0)
            {
                TempData["Error"] = "لا توجد بيانات للاستيراد";
                return RedirectToAction(nameof(Create));
            }

            int imported = 0;
            foreach (var row in rows)
            {
                Validate(row);
                if (!row.IsValid) continue;

                var program = new TrainingProgram
                {
                    ProgramCode = row.ProgramCode?.Trim(),
                    Title = row.Title!.Trim(),
                    Description = row.Description!.Trim(),
                    Categories = row.Categories?.Trim(),
                    ProgramType = row.ProgramType?.Trim(),
                    TargetAudience = row.TargetAudience?.Trim(),
                    Duration = row.Duration!.Trim(),
                    Instructor = row.Instructor!.Trim(),
                    Location = row.Location!.Trim(),
                    Objectives = row.Objectives?.Trim(),
                    Topics = row.Topics?.Trim(),
                    Prerequisites = row.Prerequisites?.Trim(),
                    Status = ProgramStatus.Active,
                    CreatedAt = DateTime.Now,
                    ReferenceNumber = await GenerateReferenceNumberAsync()
                };

                _context.TrainingPrograms.Add(program);
                await _context.SaveChangesAsync();
                imported++;
            }

            if (imported > 0)
            {
                TempData["Success"] = $"تم استيراد {imported} برنامج بنجاح";
            }
            else
            {
                TempData["Error"] = "لم يتم استيراد أي برنامج. تحقق من صحة البيانات";
            }

            return RedirectToAction(nameof(Index));
        }

        // قراءة صفوف Excel وتحويلها إلى قائمة
        private List<ProgramImportRow> ParseExcel(IFormFile file)
        {
            var rows = new List<ProgramImportRow>();

            using var stream = new MemoryStream();
            file.CopyTo(stream);
            stream.Position = 0;

            using var workbook = new XLWorkbook(stream);
            var ws = workbook.Worksheet(1);
            var range = ws.RangeUsed();
            if (range == null) return rows;

            var lastRow = range.RowCount();
            // نبدأ من الصف الثاني (بعد الترويسة)
            for (int r = 2; r <= lastRow; r++)
            {
                string Cell(int c) => ws.Cell(r, c).GetString().Trim();

                var row = new ProgramImportRow
                {
                    RowNumber = r,
                    ProgramCode = Cell(1),
                    Title = Cell(2),
                    Description = Cell(3),
                    Categories = Cell(4),
                    ProgramType = Cell(5),
                    TargetAudience = Cell(6),
                    Duration = Cell(7),
                    Instructor = Cell(8),
                    Location = Cell(9),
                    Objectives = Cell(10),
                    Topics = Cell(11),
                    Prerequisites = Cell(12)
                };

                // تجاهل الصفوف الفارغة تماماً
                if (string.IsNullOrWhiteSpace(row.Title) &&
                    string.IsNullOrWhiteSpace(row.Description) &&
                    string.IsNullOrWhiteSpace(row.Duration) &&
                    string.IsNullOrWhiteSpace(row.Instructor) &&
                    string.IsNullOrWhiteSpace(row.Location))
                {
                    continue;
                }

                Validate(row);
                rows.Add(row);
            }

            return rows;
        }

        // التحقق من الحقول المطلوبة
        private void Validate(ProgramImportRow row)
        {
            row.Errors.Clear();

            if (string.IsNullOrWhiteSpace(row.Title))
                row.Errors.Add("عنوان البرنامج مطلوب");
            if (string.IsNullOrWhiteSpace(row.Description))
                row.Errors.Add("وصف البرنامج مطلوب");
            if (string.IsNullOrWhiteSpace(row.Duration))
                row.Errors.Add("المدة مطلوبة");
            if (string.IsNullOrWhiteSpace(row.Instructor))
                row.Errors.Add("المدرب مطلوب");
            if (string.IsNullOrWhiteSpace(row.Location))
                row.Errors.Add("الموقع مطلوب");

            row.IsValid = row.Errors.Count == 0;
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

            var existingProgram = await _context.TrainingPrograms.FindAsync(id);
            if (existingProgram == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                existingProgram.ProgramCode = program.ProgramCode?.Trim();
                existingProgram.Title = program.Title.Trim();
                existingProgram.Description = program.Description.Trim();
                existingProgram.Categories = program.Categories?.Trim();
                existingProgram.ProgramType = program.ProgramType?.Trim();
                existingProgram.TargetAudience = program.TargetAudience?.Trim();
                existingProgram.Duration = program.Duration.Trim();
                existingProgram.Instructor = program.Instructor.Trim();
                existingProgram.Location = program.Location.Trim();
                existingProgram.Logo = program.Logo?.Trim();
                existingProgram.Objectives = program.Objectives?.Trim();
                existingProgram.Topics = program.Topics?.Trim();
                existingProgram.Prerequisites = program.Prerequisites?.Trim();
                // ReferenceNumber و CreatedAt و Status لا تُقبل من نموذج التعديل.

                await _context.SaveChangesAsync();
                TempData["Success"] = "تم تحديث البرنامج بنجاح";
                return RedirectToAction(nameof(Index));
            }

            program.ReferenceNumber = existingProgram.ReferenceNumber;
            program.CreatedAt = existingProgram.CreatedAt;
            program.Status = existingProgram.Status;
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
                var hasBatches = await _context.Batches.AnyAsync(b => b.TrainingProgramId == id);
                if (hasBatches)
                {
                    TempData["Error"] = "لا يمكن حذف برنامج مرتبط بدفعات. يمكنك إيقاف البرنامج بدلاً من ذلك.";
                    return RedirectToAction(nameof(Index));
                }

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
    }
}
