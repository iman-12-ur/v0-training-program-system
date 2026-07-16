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

        // تصدير التقرير إلى CSV
        public async Task<IActionResult> ExportToCsv(string type = "registrations")
        {
            var csv = new System.Text.StringBuilder();
            
            if (type == "registrations")
            {
                csv.AppendLine("الاسم,الرقم الوظيفي,البريد الإلكتروني,الهاتف,البرنامج,الدفعة,الحالة,تاريخ التسجيل");

                var registrations = await _context.Registrations
                    .Include(r => r.Batch)
                    .ThenInclude(b => b.TrainingProgram)
                    .ToListAsync();

                foreach (var r in registrations)
                {
                    var status = r.Status switch
                    {
                        RegistrationStatus.Pending => "قيد المراجعة",
                        RegistrationStatus.Approved => "مقبول",
                        RegistrationStatus.Rejected => "مرفوض",
                        _ => "غير معروف"
                    };
                    csv.AppendLine($"{r.VisitorName},{r.EmployeeId},{r.Email},{r.Phone},{r.Batch?.TrainingProgram?.Title},{r.Batch?.Name},{status},{r.RegisteredAt:yyyy-MM-dd}");
                }
            }
            else if (type == "programs")
            {
                csv.AppendLine("البرنامج,عدد الدفعات,عدد التسجيلات,الحالة");

                var programs = await _context.TrainingPrograms
                    .Include(p => p.Batches)
                    .ThenInclude(b => b.Registrations)
                    .ToListAsync();

                foreach (var p in programs)
                {
                    var totalRegs = p.Batches?.Sum(b => b.Registrations?.Count ?? 0) ?? 0;
                    csv.AppendLine($"{p.Title},{p.Batches?.Count ?? 0},{totalRegs},{(p.Status == ProgramStatus.Active ? "نشط" : "غير نشط")}");
                }
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
            var bom = new byte[] { 0xEF, 0xBB, 0xBF };
            var result = bom.Concat(bytes).ToArray();

            return File(result, "text/csv", $"report_{type}_{DateTime.Now:yyyyMMdd}.csv");
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
