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

        public async Task<IActionResult> Index(string? search, string? category)
        {
            var settings = await _context.SystemSettings.FirstOrDefaultAsync() ?? new SystemSettings();
            
            var query = _context.TrainingPrograms
                .Include(p => p.Batches)
                .Where(p => p.Status == ProgramStatus.Active);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Title.Contains(search) || p.Description.Contains(search));
            }

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(p => p.Categories != null && p.Categories.Contains(category));
            }

            var programs = await query.OrderByDescending(p => p.CreatedAt).ToListAsync();

            var allCategories = await _context.TrainingPrograms
                .Where(p => p.Categories != null)
                .Select(p => p.Categories!)
                .ToListAsync();

            var categories = allCategories
                .SelectMany(c => c.Split(',', StringSplitOptions.RemoveEmptyEntries))
                .Select(c => c.Trim())
                .Distinct()
                .ToList();

            var viewModel = new HomeViewModel
            {
                Programs = programs,
                WelcomeTitle = settings.WelcomeTitle,
                WelcomeDescription = settings.WelcomeDescription,
                SearchQuery = search,
                CategoryFilter = category,
                AllCategories = categories
            };

            return View(viewModel);
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

            var availableBatches = program.Batches
                .Where(b => b.Status == BatchStatus.Upcoming && b.CurrentParticipants < b.MaxParticipants)
                .ToList();

            var viewModel = new ProgramDetailsViewModel
            {
                Program = program,
                AvailableBatches = availableBatches
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Register(int programId, int batchId)
        {
            var program = await _context.TrainingPrograms.FindAsync(programId);
            var batch = await _context.Batches.FindAsync(batchId);

            if (program == null || batch == null)
            {
                return NotFound();
            }

            var viewModel = new RegisterViewModel
            {
                Program = program,
                Batch = batch,
                Registration = new Registration { BatchId = batchId }
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            var program = await _context.TrainingPrograms.FindAsync(model.Registration.BatchId);
            var batch = await _context.Batches.FindAsync(model.Registration.BatchId);

            if (batch == null)
            {
                return NotFound();
            }

            // Check for duplicate registration
            var existingRegistration = await _context.Registrations
                .AnyAsync(r => r.EmployeeId == model.Registration.EmployeeId && r.BatchId == model.Registration.BatchId);

            if (existingRegistration)
            {
                ModelState.AddModelError("", "لقد قمت بالتسجيل في هذه الدفعة مسبقاً");
                model.Batch = batch;
                model.Program = (await _context.Batches.Include(b => b.TrainingProgram).FirstAsync(b => b.Id == batch.Id)).TrainingProgram!;
                return View(model);
            }

            if (ModelState.IsValid)
            {
                model.Registration.RegisteredAt = DateTime.Now;
                model.Registration.Status = RegistrationStatus.Pending;

                _context.Registrations.Add(model.Registration);
                
                batch.CurrentParticipants++;
                _context.Batches.Update(batch);
                
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(RegistrationSuccess));
            }

            model.Batch = batch;
            model.Program = (await _context.Batches.Include(b => b.TrainingProgram).FirstAsync(b => b.Id == batch.Id)).TrainingProgram!;
            return View(model);
        }

        public IActionResult RegistrationSuccess()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
