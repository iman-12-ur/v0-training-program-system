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
            var query = _context.Registrations
                .Include(r => r.Batch)
                .ThenInclude(b => b!.TrainingProgram)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<RegistrationStatus>(status, out var statusEnum))
            {
                query = query.Where(r => r.Status == statusEnum);
            }

            if (programId.HasValue && programId.Value > 0)
            {
                query = query.Where(r => r.Batch != null && r.Batch.TrainingProgramId == programId.Value);
            }

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(r => 
                    r.VisitorName.Contains(search) || 
                    r.EmployeeId.Contains(search) ||
                    r.Email.Contains(search) ||
                    (r.JobTitle != null && r.JobTitle.Contains(search)));
            }

            var registrations = await query
                .OrderByDescending(r => r.RegisteredAt)
                .ToListAsync();

            // قائمة البرامج للفلترة
            ViewBag.Programs = await _context.TrainingPrograms
                .OrderBy(p => p.Title)
                .Select(p => new { p.Id, p.Title })
                .ToListAsync();

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

        // تصدير التقارير - متاح للجميع
        public async Task<IActionResult> Export()
        {
            var registrations = await _context.Registrations
                .Include(r => r.Batch)
                .ThenInclude(b => b!.TrainingProgram)
                .OrderByDescending(r => r.RegisteredAt)
                .ToListAsync();

            // Generate CSV
            var csv = "الاسم,الرقم الوظيفي,المسمى الوظيفي,الدائرة,القسم,البريد,الهاتف,البرنامج,الدفعة,الحالة,تاريخ التسجيل\n";
            foreach (var r in registrations)
            {
                csv += $"{r.VisitorName},{r.EmployeeId},{r.JobTitle},{r.Court},{r.Department},{r.Email},{r.Phone},{r.Batch?.TrainingProgram?.Title},{r.Batch?.Name},{r.Status},{r.RegisteredAt:yyyy-MM-dd}\n";
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
            return File(bytes, "text/csv", $"registrations_{DateTime.Now:yyyyMMdd}.csv");
        }

        // تصدير المقبولين حسب البرنامج - متاح للجميع
        public async Task<IActionResult> ExportApproved(int? programId = null)
        {
            var query = _context.Registrations
                .Include(r => r.Batch)
                .ThenInclude(b => b!.TrainingProgram)
                .Where(r => r.Status == RegistrationStatus.Approved)
                .AsQueryable();

            string programTitle = "جميع_البرامج";
            if (programId.HasValue && programId.Value > 0)
            {
                query = query.Where(r => r.Batch != null && r.Batch.TrainingProgramId == programId.Value);
                var program = await _context.TrainingPrograms.FindAsync(programId.Value);
                if (program != null)
                {
                    programTitle = program.Title.Replace(" ", "_");
                }
            }

            var registrations = await query
                .OrderBy(r => r.Batch!.TrainingProgram!.Title)
                .ThenBy(r => r.Batch!.Name)
                .ThenByDescending(r => r.RegisteredAt)
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("المقبولون");
            ws.RightToLeft = true;

            var headers = new[]
            {
                "م", "الاسم", "الرقم الوظيفي", "المسمى الوظيفي", "الدائرة/المحكمة",
                "القسم", "البريد الإلكتروني", "الهاتف", "البرنامج", "الدفعة", "تاريخ التسجيل"
            };

            // ترويسة الأعمدة
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cell(1, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1e3a5f");
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            }

            // بيانات المقبولين
            int row = 2;
            int seq = 1;
            foreach (var r in registrations)
            {
                ws.Cell(row, 1).Value = seq++;
                ws.Cell(row, 2).Value = r.VisitorName;
                ws.Cell(row, 3).Value = r.EmployeeId;
                ws.Cell(row, 4).Value = r.JobTitle ?? "";
                ws.Cell(row, 5).Value = r.Court;
                ws.Cell(row, 6).Value = r.Department;
                ws.Cell(row, 7).Value = r.Email;
                ws.Cell(row, 8).Value = r.Phone;
                ws.Cell(row, 9).Value = r.Batch?.TrainingProgram?.Title ?? "";
                ws.Cell(row, 10).Value = r.Batch?.Name ?? "";
                ws.Cell(row, 11).Value = r.RegisteredAt.ToString("yyyy-MM-dd");
                row++;
            }

            // حدود للجدول وتنسيق
            var usedRange = ws.Range(1, 1, Math.Max(row - 1, 1), headers.Length);
            usedRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            usedRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"accepted_{programTitle}_{DateTime.Now:yyyyMMdd}.xlsx");
        }
    }
}
