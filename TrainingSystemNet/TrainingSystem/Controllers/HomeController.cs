using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.Data;
using TrainingSystem.Models;

namespace TrainingSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var settings = await _context.SystemSettings.FirstOrDefaultAsync();
            ViewBag.WelcomeTitle = settings?.WelcomeTitle ?? "مرحباً بك في بوابة التدريب";
            ViewBag.WelcomeDescription = settings?.WelcomeDescription ?? "استعرض البرامج التدريبية المتاحة";

            var programs = await _context.TrainingPrograms
                .Where(p => p.Status == ProgramStatus.Active)
                .Include(p => p.Batches)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(programs);
        }

        public async Task<IActionResult> ProgramDetails(int id)
        {
            var program = await _context.TrainingPrograms
                .Include(p => p.Batches)
                .FirstOrDefaultAsync(p => p.Id == id && p.Status == ProgramStatus.Active);

            if (program == null)
            {
                return NotFound();
            }

            // عدد المقبولين لكل دفعة (لعرض امتلاء المقاعد)
            var batchIds = program.Batches.Select(b => b.Id).ToList();
            var approvedCounts = await _context.Registrations
                .Where(r => batchIds.Contains(r.BatchId)
                            && r.Status == RegistrationStatus.Approved)
                .GroupBy(r => r.BatchId)
                .Select(g => new { BatchId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.BatchId, x => x.Count);

            ViewBag.ApprovedCounts = approvedCounts;

            return View(program);
        }

        [HttpGet]
        public async Task<IActionResult> Register(int programId, int batchId)
        {
            var batch = await GetAvailableBatchAsync(batchId);
            if (batch == null || batch.TrainingProgramId != programId)
            {
                TempData["Error"] = "البرنامج أو الدفعة المطلوبة غير متاحة للتسجيل حالياً.";
                return RedirectToAction(nameof(ProgramDetails), new { id = programId });
            }

            SetRegistrationViewData(batch);
            return View(new Registration { BatchId = batchId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(Registration registration, string? employeeIdSuffix)
        {
            var batch = await GetAvailableBatchAsync(registration.BatchId);
            if (batch == null)
            {
                ModelState.AddModelError(string.Empty, "هذه الدفعة غير متاحة للتسجيل حالياً.");
            }

            // أول ثلاثة أرقام ثابتة، ويُحفظ الرقم الوظيفي كاملاً بصيغة 192 + الأرقام المدخلة.
            var normalizedSuffix = ConvertArabicToEnglish(employeeIdSuffix ?? string.Empty).Trim();
            registration.EmployeeId = $"192{normalizedSuffix}";
            registration.Phone = ConvertArabicToEnglish(registration.Phone ?? string.Empty).Trim();
            registration.VisitorName = registration.VisitorName?.Trim() ?? string.Empty;
            registration.Email = registration.Email?.Trim() ?? string.Empty;
            registration.JobTitle = registration.JobTitle?.Trim();
            registration.Court = registration.Court?.Trim() ?? string.Empty;
            registration.Department = registration.Department?.Trim() ?? string.Empty;

            // استبدال نتيجة التحقق الأولية لأن EmployeeId يُركّب في الخادم من الجزء الثابت والمتغير.
            ModelState.Remove(nameof(Registration.EmployeeId));
            if (string.IsNullOrWhiteSpace(normalizedSuffix) || !normalizedSuffix.All(char.IsAsciiDigit))
            {
                ModelState.AddModelError(nameof(Registration.EmployeeId), "أدخل الأرقام المتبقية من الرقم الوظيفي باستخدام أرقام إنجليزية فقط");
            }

            if (await _context.Registrations.AnyAsync(r =>
                    r.EmployeeId == registration.EmployeeId && r.BatchId == registration.BatchId))
            {
                ModelState.AddModelError(nameof(Registration.EmployeeId), "تم التسجيل مسبقاً بهذا الرقم الوظيفي في هذه الدفعة");
            }

            if (ModelState.IsValid && batch != null)
            {
                // لا نثق بأي حالة أو بيانات قرار قادمة من المتصفح.
                registration.Status = RegistrationStatus.Pending;
                registration.RegisteredAt = DateTime.Now;
                registration.ApprovedBy = null;
                registration.ApprovedAt = null;
                registration.Notes = null;

                _context.Registrations.Add(registration);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(RegistrationSuccess));
            }

            if (batch != null)
            {
                SetRegistrationViewData(batch);
            }

            return View(registration);
        }

        private Task<Batch?> GetAvailableBatchAsync(int batchId)
        {
            return _context.Batches
                .Include(b => b.TrainingProgram)
                .FirstOrDefaultAsync(b =>
                    b.Id == batchId
                    && b.Status == BatchStatus.Upcoming
                    && b.EndDate.Date >= DateTime.Today
                    && b.TrainingProgram != null
                    && b.TrainingProgram.Status == ProgramStatus.Active);
        }

        private void SetRegistrationViewData(Batch batch)
        {
            ViewBag.Program = batch.TrainingProgram;
            ViewBag.Batch = batch;
        }

        public IActionResult RegistrationSuccess()
        {
            return View();
        }

        private string ConvertArabicToEnglish(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            var arabicNumerals = new[] { '٠', '١', '٢', '٣', '٤', '٥', '٦', '٧', '٨', '٩' };
            var result = input.ToCharArray();

            for (int i = 0; i < result.Length; i++)
            {
                int index = Array.IndexOf(arabicNumerals, result[i]);
                if (index >= 0)
                {
                    result[i] = index.ToString()[0];
                }
            }

            return new string(result);
        }
    }
}
