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
                .FirstOrDefaultAsync(p => p.Id == id);

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
            var batch = await _context.Batches
                .Include(b => b.TrainingProgram)
                .FirstOrDefaultAsync(b => b.Id == batchId);

            if (batch == null)
            {
                return NotFound();
            }

            ViewBag.Program = batch.TrainingProgram;
            ViewBag.Batch = batch;

            return View(new Registration { BatchId = batchId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(Registration registration)
        {
            // Convert Arabic numbers to English
            registration.EmployeeId = ConvertArabicToEnglish(registration.EmployeeId);
            registration.Phone = ConvertArabicToEnglish(registration.Phone);

            // Check for duplicate registration
            var existingReg = await _context.Registrations
                .AnyAsync(r => r.EmployeeId == registration.EmployeeId && r.BatchId == registration.BatchId);

            if (existingReg)
            {
                ModelState.AddModelError("EmployeeId", "تم التسجيل مسبقاً بهذا الرقم الوظيفي في هذه الدفعة");
            }

            if (ModelState.IsValid)
            {
                registration.Status = RegistrationStatus.Pending;
                registration.RegisteredAt = DateTime.Now;

                _context.Registrations.Add(registration);
                
                // Update batch participants count
                var batch = await _context.Batches.FindAsync(registration.BatchId);
                if (batch != null)
                {
                    batch.CurrentParticipants++;
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(RegistrationSuccess));
            }

            var batchData = await _context.Batches
                .Include(b => b.TrainingProgram)
                .FirstOrDefaultAsync(b => b.Id == registration.BatchId);

            ViewBag.Program = batchData?.TrainingProgram;
            ViewBag.Batch = batchData;

            return View(registration);
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
