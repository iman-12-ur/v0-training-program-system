using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.Data;
using TrainingSystem.Models;
using TrainingSystem.ViewModels;

namespace TrainingSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // الصفحة الرئيسية العامة - عرض البرامج للموظفين
        public async Task<IActionResult> Index(string? search, string? category)
        {
            var settings = await _context.SystemSettings.FirstOrDefaultAsync() ?? new SystemSettings();
            
            var query = _context.TrainingPrograms
                .Include(p => p.Batches)
                .Where(p => p.Status == ProgramStatus.Active);

            // البحث
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => 
                    p.Title.Contains(search) || 
                    p.Description.Contains(search) ||
                    p.Instructor.Contains(search));
            }

            // فلترة حسب التصنيف
            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(p => p.Categories != null && p.Categories.Contains(category));
            }

            var programs = await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
            
            // جمع جميع التصنيفات
            var allCategories = await _context.TrainingPrograms
                .Where(p => p.Categories != null)
                .Select(p => p.Categories!)
                .ToListAsync();
            
            var categories = allCategories
                .SelectMany(c => c.Split(',', StringSplitOptions.RemoveEmptyEntries))
                .Select(c => c.Trim())
                .Distinct()
                .ToList();

            var viewModel = new PublicProgramViewModel
            {
                Programs = programs,
                Settings = settings,
                SearchQuery = search,
                CategoryFilter = category,
                AllCategories = categories
            };

            return View(viewModel);
        }

        // عرض تفاصيل البرنامج
        public async Task<IActionResult> ProgramDetails(int id)
        {
            var program = await _context.TrainingPrograms
                .Include(p => p.Batches)
                .FirstOrDefaultAsync(p => p.Id == id && p.Status == ProgramStatus.Active);

            if (program == null)
            {
                return NotFound();
            }

            return View(program);
        }

        // نموذج التسجيل
        [HttpGet]
        public async Task<IActionResult> Register(int programId, int? batchId)
        {
            var program = await _context.TrainingPrograms
                .Include(p => p.Batches)
                .FirstOrDefaultAsync(p => p.Id == programId && p.Status == ProgramStatus.Active);

            if (program == null)
            {
                return NotFound();
            }

            var availableBatches = program.Batches
                .Where(b => b.Status == BatchStatus.Upcoming && !b.IsFull)
                .ToList();

            var viewModel = new RegistrationFormViewModel
            {
                Program = program,
                AvailableBatches = availableBatches,
                Registration = new Registration
                {
                    TrainingProgramId = programId,
                    BatchId = batchId ?? 0
                }
            };

            return View(viewModel);
        }

        // حفظ التسجيل
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegistrationFormViewModel model)
        {
            // التحقق من البرنامج
            var program = await _context.TrainingPrograms
                .Include(p => p.Batches)
                .FirstOrDefaultAsync(p => p.Id == model.Registration.TrainingProgramId);

            if (program == null)
            {
                return NotFound();
            }

            // التحقق من الدفعة
            var batch = program.Batches.FirstOrDefault(b => b.Id == model.Registration.BatchId);
            if (batch == null || batch.IsFull)
            {
                ModelState.AddModelError("", "الدفعة المختارة غير متاحة أو مكتملة العدد");
                model.Program = program;
                model.AvailableBatches = program.Batches.Where(b => b.Status == BatchStatus.Upcoming && !b.IsFull).ToList();
                return View(model);
            }

            // التحقق من عدم وجود تسجيل سابق
            var existingRegistration = await _context.Registrations
                .AnyAsync(r => r.EmployeeId == model.Registration.EmployeeId && r.BatchId == model.Registration.BatchId);

            if (existingRegistration)
            {
                ModelState.AddModelError("Registration.EmployeeId", "تم التسجيل مسبقاً بهذا الرقم الوظيفي في هذه الدفعة");
                model.Program = program;
                model.AvailableBatches = program.Batches.Where(b => b.Status == BatchStatus.Upcoming && !b.IsFull).ToList();
                return View(model);
            }

            if (ModelState.IsValid)
            {
                model.Registration.Status = RegistrationStatus.Pending;
                model.Registration.RegisteredAt = DateTime.Now;

                _context.Registrations.Add(model.Registration);
                await _context.SaveChangesAsync();

                TempData["Success"] = "تم تقديم طلب الترشيح بنجاح. سيتم إبلاغك بالنتيجة عبر البريد الإلكتروني.";
                return RedirectToAction(nameof(RegistrationSuccess), new { id = model.Registration.Id });
            }

            model.Program = program;
            model.AvailableBatches = program.Batches.Where(b => b.Status == BatchStatus.Upcoming && !b.IsFull).ToList();
            return View(model);
        }

        // صفحة نجاح التسجيل
        public async Task<IActionResult> RegistrationSuccess(int id)
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

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}
