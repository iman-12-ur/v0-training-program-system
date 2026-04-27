using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using TrainingSystem.Data;
using TrainingSystem.Models;

namespace TrainingSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, IConfiguration configuration)
        {
            _logger = logger;
            _context = context;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            var programs = await _context.TrainingPrograms
                .Include(p => p.Batches)
                .Where(p => p.Status == ProgramStatus.Active)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            ViewBag.WelcomeTitle = _configuration["AppSettings:WelcomeTitle"];
            ViewBag.WelcomeDescription = _configuration["AppSettings:WelcomeDescription"];

            return View(programs);
        }

        public async Task<IActionResult> Details(int id)
        {
            var program = await _context.TrainingPrograms
                .Include(p => p.Batches)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (program == null)
            {
                return NotFound();
            }

            return View(program);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
