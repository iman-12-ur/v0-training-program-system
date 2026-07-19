using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.Data;
using TrainingSystem.Models;

namespace TrainingSystem.Controllers
{
    // السماح لجميع الأدوار بالعرض
    [Authorize(Roles = "SuperAdmin,Admin,Supervisor")]
    public class RegistrationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public RegistrationsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // عرض الطلبات - متاح للجميع
        public async Task<IActionResult> Index(string? status = null, string? search = null, int programId = 0)
        {
            var query = _context.Registrations
                .AsNoTracking()
                .Include(r => r.Batch)
                .ThenInclude(b => b!.TrainingProgram)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<RegistrationStatus>(status, true, out var statusEnum))
            {
                query = query.Where(r => r.Status == statusEnum);
            }

            if (programId > 0)
            {
                // التسجيل مرتبط بالبرنامج من خلال الدفعة؛ نستخدم BatchId صراحة لضمان الفلترة الصحيحة.
                query = query.Where(r => _context.Batches.Any(b =>
                    b.Id == r.BatchId && b.TrainingProgramId == programId));
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchTerm = search.Trim();
                query = query.Where(r =>
                    r.VisitorName.Contains(searchTerm) ||
                    r.EmployeeId.Contains(searchTerm) ||
                    r.Email.Contains(searchTerm) ||
                    (r.JobTitle != null && r.JobTitle.Contains(searchTerm)) ||
                    r.Court.Contains(searchTerm) ||
                    r.Department.Contains(searchTerm) ||
                    (r.Batch != null && r.Batch.TrainingProgram != null &&
                     r.Batch.TrainingProgram.Title.Contains(searchTerm)));
            }

            var registrations = await query
                .OrderByDescending(r => r.RegisteredAt)
                .ToListAsync();

            // نمرر كيانات عامة إلى Razor بدلاً من نوع مجهول داخل ViewBag.
            ViewBag.Programs = await _context.TrainingPrograms
                .AsNoTracking()
                .OrderBy(p => p.Title)
                .ToListAsync();
            ViewBag.CurrentStatus = status ?? string.Empty;
            ViewBag.Search = search ?? string.Empty;
            ViewBag.CurrentProgramId = programId;
            ViewBag.FilteredCount = registrations.Count;

            return View(registrations);
        }

        // تفاصيل الطلب - متاح للجميع
        public async Task<IActionResult> Details(int id)
        {
            var registration = await _context.Registrations
                .Include(r => r.Batch)
                .ThenInclude(b => b!.TrainingProgram)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (registration == null)
            {
                return NotFound();
            }

            return View(registration);
        }

        // قبول الطلب - SuperAdmin و Admin و Supervisor
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Admin,Supervisor")]
        public async Task<IActionResult> Approve(int id)
        {
            var registration = await _context.Registrations.FindAsync(id);
            if (registration != null)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                
                registration.Status = RegistrationStatus.Approved;
                registration.ApprovedBy = currentUser?.FullName ?? "المدير";
                registration.ApprovedAt = DateTime.Now;

                await _context.SaveChangesAsync();
                TempData["Success"] = "تم قبول الطلب بنجاح";
            }

            return RedirectToAction(nameof(Index));
        }

        // رفض الطلب - SuperAdmin و Admin و Supervisor
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Admin,Supervisor")]
        public async Task<IActionResult> Reject(int id, string? notes = null)
        {
            var registration = await _context.Registrations
                .Include(r => r.Batch)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (registration != null)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                
                registration.Status = RegistrationStatus.Rejected;
                registration.ApprovedBy = currentUser?.FullName ?? "المدير";
                registration.ApprovedAt = DateTime.Now;
                registration.Notes = notes;

                // Decrease batch participants count
                if (registration.Batch != null && registration.Batch.CurrentParticipants > 0)
                {
                    registration.Batch.CurrentParticipants--;
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "تم رفض الطلب";
            }

            return RedirectToAction(nameof(Index));
        }

        // حذف الطلب - SuperAdmin و Admin فقط
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var registration = await _context.Registrations
                .Include(r => r.Batch)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (registration != null)
            {
                // Decrease batch participants count
                if (registration.Batch != null && registration.Batch.CurrentParticipants > 0)
                {
                    registration.Batch.CurrentParticipants--;
                }

                _context.Registrations.Remove(registration);
                await _context.SaveChangesAsync();
                TempData["Success"] = "تم حذف الطلب";
            }

            return RedirectToAction(nameof(Index));
        }

        // تصدير جميع الطلبات إلى ملف Excel حقيقي - متاح للجميع
        public async Task<IActionResult> Export()
        {
            var registrations = await _context.Registrations
                .Include(r => r.Batch)
                .ThenInclude(b => b!.TrainingProgram)
                .OrderByDescending(r => r.RegisteredAt)
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("جميع الطلبات");
            worksheet.RightToLeft = true;

            string[] headers =
            {
                "م", "الاسم", "الرقم الوظيفي", "المسمى الوظيفي", "الدائرة/المحكمة",
                "القسم", "البريد الإلكتروني", "الهاتف", "البرنامج", "الدفعة", "الحالة", "تاريخ التسجيل"
            };

            for (int column = 1; column <= headers.Length; column++)
            {
                var cell = worksheet.Cell(1, column);
                cell.Value = headers[column - 1];
                cell.Style.Font.Bold = true;
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1e3a5f");
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

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

            int lastRow = Math.Max(registrations.Count + 1, 1);
            var tableRange = worksheet.Range(1, 1, lastRow, headers.Length);
            tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            tableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            tableRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.SheetView.FreezeRows(1);
            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"جميع-طلبات-الترشيح-{DateTime.Now:yyyyMMdd}.xlsx");
        }

        // تصدير جميع حالات مرشحي البرنامج المحدد: قيد المراجعة والمقبول والمرفوض
        public async Task<IActionResult> ExportByProgram(int programId)
        {
            if (programId <= 0)
            {
                TempData["Error"] = "يرجى اختيار برنامج محدد أولاً، ثم الضغط على تصدير جميع الحالات للبرنامج المحدد.";
                return RedirectToAction(nameof(Index));
            }

            var selectedProgram = await _context.TrainingPrograms
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == programId);

            if (selectedProgram == null)
            {
                TempData["Error"] = "البرنامج المحدد غير موجود.";
                return RedirectToAction(nameof(Index));
            }

            var registrations = await _context.Registrations
                .AsNoTracking()
                .Include(r => r.Batch)
                .ThenInclude(b => b!.TrainingProgram)
                .Where(r => _context.Batches.Any(b =>
                    b.Id == r.BatchId && b.TrainingProgramId == programId))
                .OrderBy(r => r.Batch!.Name)
                .ThenBy(r => r.VisitorName)
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("مرشحو البرنامج");
            worksheet.RightToLeft = true;

            string[] headers =
            {
                "م", "الاسم", "الرقم الوظيفي", "المسمى الوظيفي", "الدائرة/المحكمة",
                "القسم", "البريد الإلكتروني", "الهاتف", "البرنامج", "الدفعة", "الحالة", "تاريخ التسجيل"
            };

            for (int column = 1; column <= headers.Length; column++)
            {
                var cell = worksheet.Cell(1, column);
                cell.Value = headers[column - 1];
                cell.Style.Font.Bold = true;
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1e3a5f");
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            }

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

            int lastRow = Math.Max(registrations.Count + 1, 1);
            var tableRange = worksheet.Range(1, 1, lastRow, headers.Length);
            tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            tableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            tableRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.SheetView.FreezeRows(1);
            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"جميع-حالات-مرشحي-البرنامج-{selectedProgram.Id}-{DateTime.Now:yyyyMMdd}.xlsx");
        }
    }
}
