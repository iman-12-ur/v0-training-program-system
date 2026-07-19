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
        public async Task<IActionResult> Index(string? status = null, string? search = null, int? programId = null)
        {
            var query = BuildFilteredQuery(status, search, programId);

            var registrations = await query
                .OrderByDescending(r => r.RegisteredAt)
                .ToListAsync();

            // قائمة البرامج للفلترة
            ViewBag.Programs = await _context.TrainingPrograms
                .OrderBy(p => p.Title)
                .Select(p => new { p.Id, p.Title })
                .ToListAsync();

            // عدد المقبولين والعدد المطلوب لكل دفعة (لعرض عدّاد المقاعد في الجدول)
            var batchIds = registrations
                .Select(r => r.BatchId)
                .Distinct()
                .ToList();

            ViewBag.BatchApprovedCounts = await _context.Registrations
                .Where(r => batchIds.Contains(r.BatchId) && r.Status == RegistrationStatus.Approved)
                .GroupBy(r => r.BatchId)
                .Select(g => new { BatchId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.BatchId, x => x.Count);

            ViewBag.CurrentStatus = status;
            ViewBag.Search = search;
            ViewBag.CurrentProgramId = programId;

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
            var registration = await _context.Registrations
                .Include(r => r.Batch)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (registration != null)
            {
                // منع القبول إذا اكتمل العدد المطلوب للدفعة
                if (registration.Batch != null && registration.Status != RegistrationStatus.Approved)
                {
                    var approvedCount = await _context.Registrations
                        .CountAsync(r => r.BatchId == registration.BatchId
                                         && r.Status == RegistrationStatus.Approved);

                    if (approvedCount >= registration.Batch.MaxParticipants)
                    {
                        TempData["Error"] = $"لا يمكن القبول: اكتمل العدد المطلوب للدفعة ({registration.Batch.MaxParticipants} مقاعد). يمكنك رفض أحد المقبولين لإتاحة مقعد.";
                        return RedirectToAction(nameof(Index));
                    }
                }

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

        // تصدير النتائج الحالية إلى Excel وفق البحث والبرنامج والحالة المحددة
        public async Task<IActionResult> Export(string? status = null, string? search = null, int? programId = null)
        {
            var registrations = await BuildFilteredQuery(status, search, programId)
                .OrderByDescending(r => r.RegisteredAt)
                .ToListAsync();

            return CreateRegistrationsWorkbook(registrations, "طلبات الترشيح", "طلبات-الترشيح");
        }

        private IQueryable<Registration> BuildFilteredQuery(string? status, string? search, int? programId)
        {
            var query = _context.Registrations
                .AsNoTracking()
                .Include(r => r.Batch)
                .ThenInclude(b => b!.TrainingProgram)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status)
                && Enum.TryParse<RegistrationStatus>(status, true, out var statusEnum))
            {
                query = query.Where(r => r.Status == statusEnum);
            }

            if (programId.HasValue && programId.Value > 0)
            {
                var selectedProgramId = programId.Value;
                query = query.Where(r => _context.Batches.Any(b =>
                    b.Id == r.BatchId && b.TrainingProgramId == selectedProgramId));
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
                    (r.Batch != null && r.Batch.TrainingProgram != null
                        && r.Batch.TrainingProgram.Title.Contains(searchTerm)));
            }

            return query;
        }

        private FileContentResult CreateRegistrationsWorkbook(
            IReadOnlyList<Registration> registrations,
            string worksheetName,
            string fileName)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(worksheetName);
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
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1e3a5f");
                cell.Style.Font.FontColor = XLColor.White;
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
                worksheet.Cell(row, 11).Value = GetStatusText(registration.Status);
                worksheet.Cell(row, 12).Value = registration.RegisteredAt;
                worksheet.Cell(row, 12).Style.DateFormat.Format = "yyyy/MM/dd HH:mm";
            }

            var usedRange = worksheet.Range(1, 1, Math.Max(registrations.Count + 1, 1), headers.Length);
            usedRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            usedRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            usedRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.SheetView.FreezeRows(1);
            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"{fileName}-{DateTime.Now:yyyyMMdd}.xlsx");
        }

        private static string GetStatusText(RegistrationStatus status) => status switch
        {
            RegistrationStatus.Pending => "قيد المراجعة",
            RegistrationStatus.Approved => "مقبول",
            RegistrationStatus.Rejected => "مرفوض",
            _ => "غير معروف"
        };
    }
}
