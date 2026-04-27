using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.Data;
using TrainingSystem.Models;
using TrainingSystem.ViewModels;

namespace TrainingSystem.Controllers
{
    [Authorize]
    [Route("Admin/[controller]")]
    public class RegistrationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public RegistrationsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // قائمة طلبات الترشيح
        [HttpGet]
        public async Task<IActionResult> Index(
            string? search,
            RegistrationStatus? status,
            int? programId,
            string sortField = "RegisteredAt",
            string sortDirection = "desc",
            int page = 1)
        {
            var query = _context.Registrations
                .Include(r => r.TrainingProgram)
                .Include(r => r.Batch)
                .AsQueryable();

            // البحث
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(r =>
                    r.EmployeeName.Contains(search) ||
                    r.EmployeeId.Contains(search) ||
                    r.Court.Contains(search) ||
                    r.Email.Contains(search));
            }

            // فلترة حسب الحالة
            if (status.HasValue)
            {
                query = query.Where(r => r.Status == status.Value);
            }

            // فلترة حسب البرنامج
            if (programId.HasValue)
            {
                query = query.Where(r => r.TrainingProgramId == programId.Value);
            }

            // الترتيب
            query = sortField switch
            {
                "EmployeeName" => sortDirection == "asc" 
                    ? query.OrderBy(r => r.EmployeeName) 
                    : query.OrderByDescending(r => r.EmployeeName),
                "EmployeeId" => sortDirection == "asc"
                    ? query.OrderBy(r => r.EmployeeId)
                    : query.OrderByDescending(r => r.EmployeeId),
                "Court" => sortDirection == "asc"
                    ? query.OrderBy(r => r.Court)
                    : query.OrderByDescending(r => r.Court),
                "Status" => sortDirection == "asc"
                    ? query.OrderBy(r => r.Status)
                    : query.OrderByDescending(r => r.Status),
                _ => sortDirection == "asc"
                    ? query.OrderBy(r => r.RegisteredAt)
                    : query.OrderByDescending(r => r.RegisteredAt)
            };

            var totalCount = await query.CountAsync();
            var pageSize = 20;
            var registrations = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var viewModel = new RegistrationListViewModel
            {
                Registrations = registrations,
                SearchQuery = search,
                StatusFilter = status,
                ProgramFilter = programId,
                AllPrograms = await _context.TrainingPrograms.ToListAsync(),
                TotalCount = totalCount,
                CurrentPage = page,
                PageSize = pageSize,
                SortField = sortField,
                SortDirection = sortDirection
            };

            return View(viewModel);
        }

        // تفاصيل طلب الترشيح
        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var registration = await _context.Registrations
                .Include(r => r.TrainingProgram)
                .Include(r => r.Batch)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (registration == null)
            {
                return NotFound();
            }

            return View(registration);
        }

        // قبول طلب الترشيح
        [HttpPost("Approve/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var registration = await _context.Registrations
                .Include(r => r.Batch)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (registration == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);

            registration.Status = RegistrationStatus.Approved;
            registration.ApprovedBy = user?.FullName ?? "مدير النظام";
            registration.ApprovedAt = DateTime.Now;

            // تحديث عدد المشاركين
            if (registration.Batch != null)
            {
                registration.Batch.CurrentParticipants++;
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "تم قبول طلب الترشيح بنجاح";
            return RedirectToAction(nameof(Index));
        }

        // رفض طلب الترشيح
        [HttpPost("Reject/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string? reason)
        {
            var registration = await _context.Registrations.FindAsync(id);
            if (registration == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);

            registration.Status = RegistrationStatus.Rejected;
            registration.ApprovedBy = user?.FullName ?? "مدير النظام";
            registration.ApprovedAt = DateTime.Now;
            registration.RejectionReason = reason;

            await _context.SaveChangesAsync();

            TempData["Success"] = "تم رفض طلب الترشيح";
            return RedirectToAction(nameof(Index));
        }

        // حذف طلب الترشيح
        [HttpPost("Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var registration = await _context.Registrations.FindAsync(id);
            if (registration == null)
            {
                return NotFound();
            }

            _context.Registrations.Remove(registration);
            await _context.SaveChangesAsync();

            TempData["Success"] = "تم حذف طلب الترشيح";
            return RedirectToAction(nameof(Index));
        }

        // تصدير إلى Excel
        [HttpGet("Export")]
        public async Task<IActionResult> ExportToExcel(int? programId, RegistrationStatus? status)
        {
            var query = _context.Registrations
                .Include(r => r.TrainingProgram)
                .Include(r => r.Batch)
                .AsQueryable();

            if (programId.HasValue)
            {
                query = query.Where(r => r.TrainingProgramId == programId.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(r => r.Status == status.Value);
            }

            var registrations = await query.OrderByDescending(r => r.RegisteredAt).ToListAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("طلبات الترشيح");

            // إعداد الاتجاه من اليمين لليسار
            worksheet.RightToLeft = true;

            // العناوين
            var headers = new[] { "م", "اسم الموظف", "الرقم الوظيفي", "الدائرة/المحكمة", "القسم", "البريد الإلكتروني", "رقم الجوال", "البرنامج", "الدفعة", "تاريخ التسجيل", "الحالة" };
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = headers[i];
                worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
            }

            // البيانات
            for (int i = 0; i < registrations.Count; i++)
            {
                var reg = registrations[i];
                worksheet.Cell(i + 2, 1).Value = i + 1;
                worksheet.Cell(i + 2, 2).Value = reg.EmployeeName;
                worksheet.Cell(i + 2, 3).Value = reg.EmployeeId;
                worksheet.Cell(i + 2, 4).Value = reg.Court;
                worksheet.Cell(i + 2, 5).Value = reg.Department;
                worksheet.Cell(i + 2, 6).Value = reg.Email;
                worksheet.Cell(i + 2, 7).Value = reg.Phone;
                worksheet.Cell(i + 2, 8).Value = reg.TrainingProgram?.Title ?? "";
                worksheet.Cell(i + 2, 9).Value = reg.Batch?.Name ?? "";
                worksheet.Cell(i + 2, 10).Value = reg.RegisteredAt.ToString("yyyy-MM-dd HH:mm");
                worksheet.Cell(i + 2, 11).Value = reg.StatusDisplay;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            var fileName = $"طلبات_الترشيح_{DateTime.Now:yyyy-MM-dd}.xlsx";
            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }
}
