using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.Data;
using TrainingSystem.Models;

namespace TrainingSystem.Controllers
{
    [Authorize(Roles = "Admin,Supervisor")]
    public class TrainingProgramsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public TrainingProgramsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: TrainingPrograms
        public async Task<IActionResult> Index()
        {
            return View(await _context.TrainingPrograms
                .Include(p => p.Batches)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync());
        }

        // GET: TrainingPrograms/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trainingProgram = await _context.TrainingPrograms
                .Include(p => p.Batches)
                .ThenInclude(b => b.Registrations)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (trainingProgram == null)
            {
                return NotFound();
            }

            return View(trainingProgram);
        }

        // GET: TrainingPrograms/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TrainingPrograms/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TrainingProgram trainingProgram, IFormFile? logoFile)
        {
            if (ModelState.IsValid)
            {
                if (logoFile != null && logoFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "logos");
                    Directory.CreateDirectory(uploadsFolder);
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + logoFile.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await logoFile.CopyToAsync(fileStream);
                    }
                    trainingProgram.Logo = "/uploads/logos/" + uniqueFileName;
                }

                trainingProgram.CreatedAt = DateTime.Now;
                trainingProgram.Status = ProgramStatus.Active;
                
                _context.Add(trainingProgram);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = "تم إضافة البرنامج التدريبي بنجاح";
                return RedirectToAction(nameof(Index));
            }
            return View(trainingProgram);
        }

        // GET: TrainingPrograms/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trainingProgram = await _context.TrainingPrograms.FindAsync(id);
            if (trainingProgram == null)
            {
                return NotFound();
            }
            return View(trainingProgram);
        }

        // POST: TrainingPrograms/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TrainingProgram trainingProgram, IFormFile? logoFile)
        {
            if (id != trainingProgram.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (logoFile != null && logoFile.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "logos");
                        Directory.CreateDirectory(uploadsFolder);
                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + logoFile.FileName;
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await logoFile.CopyToAsync(fileStream);
                        }
                        trainingProgram.Logo = "/uploads/logos/" + uniqueFileName;
                    }

                    _context.Update(trainingProgram);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "تم تحديث البرنامج التدريبي بنجاح";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TrainingProgramExists(trainingProgram.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(trainingProgram);
        }

        // POST: TrainingPrograms/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var trainingProgram = await _context.TrainingPrograms.FindAsync(id);
            if (trainingProgram != null)
            {
                _context.TrainingPrograms.Remove(trainingProgram);
                await _context.SaveChangesAsync();
                TempData["Success"] = "تم حذف البرنامج التدريبي بنجاح";
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: TrainingPrograms/ToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var program = await _context.TrainingPrograms.FindAsync(id);
            if (program != null)
            {
                program.Status = program.Status == ProgramStatus.Active 
                    ? ProgramStatus.Inactive 
                    : ProgramStatus.Active;
                await _context.SaveChangesAsync();
                TempData["Success"] = program.Status == ProgramStatus.Active 
                    ? "تم تفعيل البرنامج" 
                    : "تم إيقاف البرنامج";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool TrainingProgramExists(int id)
        {
            return _context.TrainingPrograms.Any(e => e.Id == id);
        }
    }
}
