using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.Data;
using TrainingSystem.Models;
using TrainingSystem.ViewModels;

namespace TrainingSystem.Controllers
{
    [Authorize]
    [Route("Admin/[controller]")]
    public class ProgramsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ProgramsController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // قائمة البرامج
        [HttpGet]
        public async Task<IActionResult> Index(string? search, string? category, ProgramStatus? status, int page = 1)
        {
            var query = _context.TrainingPrograms.Include(p => p.Batches).AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Title.Contains(search) || p.Description.Contains(search));
            }

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(p => p.Categories != null && p.Categories.Contains(category));
            }

            if (status.HasValue)
            {
                query = query.Where(p => p.Status == status.Value);
            }

            var totalCount = await query.CountAsync();
            var pageSize = 10;
            var programs = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var allCategories = await _context.TrainingPrograms
                .Where(p => p.Categories != null)
                .Select(p => p.Categories!)
                .ToListAsync();

            var categories = allCategories
                .SelectMany(c => c.Split(',', StringSplitOptions.RemoveEmptyEntries))
                .Select(c => c.Trim())
                .Distinct()
                .ToList();

            var viewModel = new ProgramListViewModel
            {
                Programs = programs,
                SearchQuery = search,
                CategoryFilter = category,
                StatusFilter = status,
                AllCategories = categories,
                TotalCount = totalCount,
                CurrentPage = page,
                PageSize = pageSize
            };

            return View(viewModel);
        }

        // إضافة برنامج - عرض النموذج
        [HttpGet("Create")]
        public IActionResult Create()
        {
            return View(new TrainingProgram());
        }

        // إضافة برنامج - حفظ
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TrainingProgram program, IFormFile? logoFile)
        {
            if (ModelState.IsValid)
            {
                // رفع الشعار
                if (logoFile != null && logoFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "logos");
                    Directory.CreateDirectory(uploadsFolder);
                    
                    var uniqueFileName = $"{Guid.NewGuid()}_{logoFile.FileName}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await logoFile.CopyToAsync(stream);
                    }
                    
                    program.LogoUrl = $"/uploads/logos/{uniqueFileName}";
                }

                program.CreatedAt = DateTime.Now;
                program.Status = ProgramStatus.Active;

                _context.TrainingPrograms.Add(program);
                await _context.SaveChangesAsync();

                TempData["Success"] = "تم إضافة البرنامج بنجاح";
                return RedirectToAction(nameof(Index));
            }

            return View(program);
        }

        // تعديل برنامج - عرض النموذج
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var program = await _context.TrainingPrograms.FindAsync(id);
            if (program == null)
            {
                return NotFound();
            }

            return View(program);
        }

        // تعديل برنامج - حفظ
        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TrainingProgram program, IFormFile? logoFile)
        {
            if (id != program.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingProgram = await _context.TrainingPrograms.FindAsync(id);
                    if (existingProgram == null)
                    {
                        return NotFound();
                    }

                    // رفع الشعار الجديد
                    if (logoFile != null && logoFile.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "logos");
                        Directory.CreateDirectory(uploadsFolder);
                        
                        var uniqueFileName = $"{Guid.NewGuid()}_{logoFile.FileName}";
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await logoFile.CopyToAsync(stream);
                        }
                        
                        existingProgram.LogoUrl = $"/uploads/logos/{uniqueFileName}";
                    }

                    existingProgram.Title = program.Title;
                    existingProgram.Description = program.Description;
                    existingProgram.Categories = program.Categories;
                    existingProgram.ProgramType = program.ProgramType;
                    existingProgram.TargetAudience = program.TargetAudience;
                    existingProgram.Duration = program.Duration;
                    existingProgram.Instructor = program.Instructor;
                    existingProgram.Location = program.Location;
                    existingProgram.Objectives = program.Objectives;
                    existingProgram.Topics = program.Topics;
                    existingProgram.Prerequisites = program.Prerequisites;
                    existingProgram.Status = program.Status;
                    existingProgram.UpdatedAt = DateTime.Now;

                    await _context.SaveChangesAsync();
                    TempData["Success"] = "تم تحديث البرنامج بنجاح";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.TrainingPrograms.AnyAsync(p => p.Id == id))
                    {
                        return NotFound();
                    }
                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(program);
        }

        // تفاصيل البرنامج
        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var program = await _context.TrainingPrograms
                .Include(p => p.Batches)
                    .ThenInclude(b => b.Registrations)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (program == null)
            {
                return NotFound();
            }

            return View(program);
        }

        // حذف برنامج
        [HttpPost("Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var program = await _context.TrainingPrograms.FindAsync(id);
            if (program == null)
            {
                return NotFound();
            }

            _context.TrainingPrograms.Remove(program);
            await _context.SaveChangesAsync();

            TempData["Success"] = "تم حذف البرنامج بنجاح";
            return RedirectToAction(nameof(Index));
        }

        // تغيير حالة البرنامج
        [HttpPost("ToggleStatus/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var program = await _context.TrainingPrograms.FindAsync(id);
            if (program == null)
            {
                return NotFound();
            }

            program.Status = program.Status == ProgramStatus.Active 
                ? ProgramStatus.Inactive 
                : ProgramStatus.Active;
            program.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Success"] = $"تم تغيير حالة البرنامج إلى {(program.Status == ProgramStatus.Active ? "نشط" : "غير نشط")}";
            return RedirectToAction(nameof(Index));
        }
    }
}
