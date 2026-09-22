using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.Data;
using TrainingSystem.Models;

namespace TrainingSystem.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin,Supervisor")]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // الصفحة الرئيسية للتقارير
        public async Task<IActionResult> Index()
        {
            var model = new ReportsViewModel
            {
                TotalPrograms = await _context.TrainingPrograms.CountAsync(),
                ActivePrograms = await _context.TrainingPrograms.CountAsync(p => p.Status == ProgramStatus.Active),
                TotalBatches = await _context.Batches.CountAsync(),
                TotalRegistrations = await _context.Registrations.CountAsync(),
                PendingRegistrations = await _context.Registrations.CountAsync(r => r.Status == RegistrationStatus.Pending),
                ApprovedRegistrations = await _context.Registrations.CountAsync(r => r.Status == RegistrationStatus.Approved),
                RejectedRegistrations = await _context.Registrations.CountAsync(r => r.Status == RegistrationStatus.Rejected),
                
                // البرامج الأكثر طلباً
                TopPrograms = await _context.Registrations
                    .Include(r => r.Batch)
                    .ThenInclude(b => b.TrainingProgram)
                    .GroupBy(r => r.Batch.TrainingProgram.Title)
                    .Select(g => new ProgramStats
                    {
                        ProgramName = g.Key,
                        TotalRegistrations = g.Count(),
                        ApprovedCount = g.Count(r => r.Status == RegistrationStatus.Approved),
                        PendingCount = g.Count(r => r.Status == RegistrationStatus.Pending),
                        RejectedCount = g.Count(r => r.Status == RegistrationStatus.Rejected)
                    })
                    .OrderByDescending(p => p.TotalRegistrations)
                    .Take(10)
                    .ToListAsync(),

                // التسجيلات الأخيرة
                RecentRegistrations = await _context.Registrations
                    .Include(r => r.Batch)
                    .ThenInclude(b => b.TrainingProgram)
                    .OrderByDescending(r => r.RegisteredAt)
                    .Take(10)
                    .ToListAsync(),

                // إحصائيات شهرية
                MonthlyStats = await _context.Registrations
                    .Where(r => r.RegisteredAt >= DateTime.Now.AddMonths(-6))
                    .GroupBy(r => new { r.RegisteredAt.Year, r.RegisteredAt.Month })
                    .Select(g => new MonthlyStats
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        Count = g.Count()
                    })
                    .OrderBy(m => m.Year)
                    .ThenBy(m => m.Month)
                    .ToListAsync()
            };

            return View(model);
        }

        // تقرير البرامج التدريبية
        public async Task<IActionResult> Programs()
        {
            var programs = await _context.TrainingPrograms
                .Include(p => p.Batches)
                .ThenInclude(b => b.Registrations)
                .ToListAsync();

            return View(programs);
        }

        // تقرير الدفعات
        public async Task<IActionResult> Batches()
        {
            var batches = await _context.Batches
                .Include(b => b.TrainingProgram)
                .Include(b => b.Registrations)
                .OrderByDescending(b => b.StartDate)
                .ToListAsync();

            return View(batches);
        }

        // تقرير التسجيلات
        public async Task<IActionResult> Registrations(string status = "all", int? programId = null)
        {
            var query = _context.Registrations
                .Include(r => r.Batch)
                .ThenInclude(b => b.TrainingProgram)
                .AsQueryable();

            if (status != "all")
            {
                if (Enum.TryParse<RegistrationStatus>(status, true, out var statusEnum))
                {
                    query = query.Where(r => r.Status == statusEnum);
                }
            }

            if (programId.HasValue)
            {
                query = query.Where(r => r.Batch.TrainingProgramId == programId);
            }

            ViewBag.Programs = await _context.TrainingPrograms.ToListAsync();
            ViewBag.SelectedStatus = status;
            ViewBag.SelectedProgramId = programId;

            var registrations = await query
                .OrderByDescending(r => r.RegisteredAt)
                .ToListAsync();

            return View(registrations);
        }

        // تصدير التقارير إلى ملف Excel حقيقي
        public async Task<IActionResult> ExportToExcel(string type = "registrations")
        {
            using var workbook = new XLWorkbook();

            if (type == "programs")
            {
                var worksheet = workbook.Worksheets.Add("البرامج");
                worksheet.RightToLeft = true;
                string[] headers = { "م", "البرنامج", "عدد الدفعات", "عدد التسجيلات", "الحالة" };
                ConfigureHeader(worksheet, headers);

                var programs = await _context.TrainingPrograms
                    .Include(p => p.Batches)
                    .ThenInclude(b => b.Registrations)
                    .OrderBy(p => p.Title)
                    .ToListAsync();

                for (int index = 0; index < programs.Count; index++)
                {
                    var program = programs[index];
                    int row = index + 2;
                    worksheet.Cell(row, 1).Value = index + 1;
                    worksheet.Cell(row, 2).Value = program.Title;
                    worksheet.Cell(row, 3).Value = program.Batches?.Count ?? 0;
                    worksheet.Cell(row, 4).Value = program.Batches?.Sum(b => b.Registrations?.Count ?? 0) ?? 0;
                    worksheet.Cell(row, 5).Value = program.Status == ProgramStatus.Active ? "نشط" : "غير نشط";
                }

                FormatWorksheet(worksheet, programs.Count + 1, headers.Length);
            }
            else
            {
                type = "registrations";
                var worksheet = workbook.Worksheets.Add("التسجيلات");
                worksheet.RightToLeft = true;
                string[] headers =
                {
                    "م", "الاسم", "الرقم الوظيفي", "المسمى الوظيفي", "الدائرة/المحكمة",
                    "القسم", "البريد الإلكتروني", "الهاتف", "البرنامج", "الدفعة", "الحالة", "تاريخ التسجيل"
                };
                ConfigureHeader(worksheet, headers);

                var registrations = await _context.Registrations
                    .Include(r => r.Batch)
                    .ThenInclude(b => b.TrainingProgram)
                    .OrderByDescending(r => r.RegisteredAt)
                    .ToListAsync();

                for (int index = 0; index < registrations.Count; index++)
                {
                    var registration = registrations[index];
                    int row = index + 2;
                    worksheet.Cell(row, 1).Value = index + 1;
                    worksheet.Cell(row, 2).Value = registration.VisitorName;
                    worksheet.Cell(row, 3).Value = registration.EmployeeId;
                    worksheet.Cell(row, 4).Value = registration.JobTitle ?? string.Empty;
                    worksheet.Cell(row, 5).Value = registration.Court;
                    worksheet.Cell(row, 6).Value = registration.Department;
                    worksheet.Cell(row, 7).Value = registration.Email;
                    worksheet.Cell(row, 8).Value = registration.Phone;
                    worksheet.Cell(row, 9).Value = registration.Batch?.TrainingProgram?.Title ?? string.Empty;
                    worksheet.Cell(row, 10).Value = registration.Batch?.Name ?? string.Empty;
                    worksheet.Cell(row, 11).Value = registration.Status switch
                    {
                        RegistrationStatus.Pending => "قيد المراجعة",
                        RegistrationStatus.Approved => "مقبول",
                        RegistrationStatus.Rejected => "مرفوض",
                        _ => "غير معروف"
                    };
                    worksheet.Cell(row, 12).Value = registration.RegisteredAt;
                    worksheet.Cell(row, 12).Style.DateFormat.Format = "yyyy/MM/dd";
                }

                FormatWorksheet(worksheet, registrations.Count + 1, headers.Length);
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var arabicFileName = type == "programs" ? "تقرير-البرامج" : "تقرير-التسجيلات";
            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"{arabicFileName}-{DateTime.Now:yyyyMMdd}.xlsx");
        }

        private static void ConfigureHeader(IXLWorksheet worksheet, IReadOnlyList<string> headers)
        {
            for (int column = 1; column <= headers.Count; column++)
            {
                var cell = worksheet.Cell(1, column);
                cell.Value = headers[column - 1];
                cell.Style.Font.Bold = true;
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1e3a5f");
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }
        }

        private static void FormatWorksheet(IXLWorksheet worksheet, int lastRow, int lastColumn)
        {
            var range = worksheet.Range(1, 1, Math.Max(lastRow, 1), lastColumn);
            range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.SheetView.FreezeRows(1);
            worksheet.Columns().AdjustToContents();
        }
    }

    // View Models
    public class ReportsViewModel
    {
        public int TotalPrograms { get; set; }
        public int ActivePrograms { get; set; }
        public int TotalBatches { get; set; }
        public int TotalRegistrations { get; set; }
        public int PendingRegistrations { get; set; }
        public int ApprovedRegistrations { get; set; }
        public int RejectedRegistrations { get; set; }
        public List<ProgramStats> TopPrograms { get; set; } = new();
        public List<Registration> RecentRegistrations { get; set; } = new();
        public List<MonthlyStats> MonthlyStats { get; set; } = new();
    }

    public class ProgramStats
    {
        public string ProgramName { get; set; } = "";
        public int TotalRegistrations { get; set; }
        public int ApprovedCount { get; set; }
        public int PendingCount { get; set; }
        public int RejectedCount { get; set; }
    }

    public class MonthlyStats
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int Count { get; set; }

        public string MonthName
        {
            get
            {
                var arabicGregorianCulture = new System.Globalization.CultureInfo("ar-OM");
                arabicGregorianCulture.DateTimeFormat.Calendar = new System.Globalization.GregorianCalendar();
                return new DateTime(Year, Month, 1).ToString("MMMM yyyy", arabicGregorianCulture);
            }
        }
    }
}
