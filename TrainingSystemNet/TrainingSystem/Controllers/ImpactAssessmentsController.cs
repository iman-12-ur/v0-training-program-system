using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.Data;
using TrainingSystem.Models;
using TrainingSystem.Services;

namespace TrainingSystem.Controllers
{
    // تقييم أثر التدريب (مباشر وبعد 90 يوماً)
    [Authorize(Roles = "SuperAdmin,Admin,Supervisor")]
    public class ImpactAssessmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ImpactAssessmentsController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private bool IsPrivileged =>
            User.IsInRole(SystemRoles.SuperAdmin) || User.IsInRole(SystemRoles.Admin);

        public async Task<IActionResult> Index(string? status)
        {
            var actor = await _userManager.GetUserAsync(User);
            var myDept = actor?.Department;

            var query = _context.TrainingImpactAssessments
                .Include(a => a.TrainingNeed)
                .AsQueryable();

            // المشرف يرى تقييمات دائرته فقط
            if (!IsPrivileged)
                query = query.Where(a => a.TrainingNeed != null && a.TrainingNeed.Department == myDept);

            if (status == "pending")
                query = query.Where(a => a.CompletedAt == null);
            else if (status == "completed")
                query = query.Where(a => a.CompletedAt != null);

            var items = await query
                .OrderBy(a => a.CompletedAt != null)
                .ThenBy(a => a.DueDate)
                .ToListAsync();

            ViewBag.Status = status;
            return View(items);
        }

        public async Task<IActionResult> Complete(int id)
        {
            var assessment = await _context.TrainingImpactAssessments
                .Include(a => a.TrainingNeed)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (assessment == null) return NotFound();
            if (!await CanAccessAsync(assessment)) return Forbid();
            return View(assessment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id, TrainingImpactAssessment model)
        {
            var assessment = await _context.TrainingImpactAssessments
                .Include(a => a.TrainingNeed)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (assessment == null) return NotFound();
            if (!await CanAccessAsync(assessment)) return Forbid();

            var actor = await _userManager.GetUserAsync(User);

            assessment.KnowledgeApplication = Math.Clamp(model.KnowledgeApplication, 1, 5);
            assessment.SkillImprovement = Math.Clamp(model.SkillImprovement, 1, 5);
            assessment.WorkImpact = Math.Clamp(model.WorkImpact, 1, 5);
            assessment.WorkQuality = Math.Clamp(model.WorkQuality, 1, 5);
            assessment.ErrorReduction = Math.Clamp(model.ErrorReduction, 1, 5);
            assessment.ProductivityGain = Math.Clamp(model.ProductivityGain, 1, 5);
            assessment.ApplicationAbility = Math.Clamp(model.ApplicationAbility, 1, 5);
            assessment.BeforeScore = Math.Clamp(model.BeforeScore, 0, 5);
            assessment.AfterScore = Math.Clamp(model.AfterScore, 0, 5);
            assessment.ManagerNotes = model.ManagerNotes?.Trim();
            assessment.CompletedAt = DateTime.Now;
            assessment.CompletedByUserId = actor?.Id;

            // إشعار مقدّم الطلب باكتمال التقييم
            if (assessment.TrainingNeed?.CreatedByUserId is string creator)
                NotificationHelper.Add(_context, creator,
                    $"اكتمل {TrainingImpactAssessment.GetTypeDisplayName(assessment.AssessmentType)} لتدريب: {assessment.TrainingNeed.SkillName}",
                    NotificationType.Success, assessment.TrainingNeedId);

            // ترقية حالة الطلب عند اكتمال كل تقييمات الأثر
            if (assessment.TrainingNeed != null)
            {
                var need = assessment.TrainingNeed;
                var siblings = await _context.TrainingImpactAssessments
                    .Where(a => a.TrainingNeedId == need.Id)
                    .ToListAsync();
                // التقييم الحالي أصبح مكتملاً في الذاكرة
                bool allDone = siblings.All(a => a.Id == assessment.Id || a.CompletedAt != null);

                if (allDone && need.ApprovalStatus == TrainingNeedApprovalStatus.ImpactAssessmentPending)
                {
                    need.ApprovalStatus = TrainingNeedApprovalStatus.ImpactAssessmentCompleted;
                    _context.TrainingNeedStatusHistories.Add(new TrainingNeedStatusHistory
                    {
                        TrainingNeedId = need.Id,
                        Status = need.ApprovalStatus,
                        Action = "اكتمال جميع تقييمات الأثر",
                        ActionByUserId = actor?.Id,
                        ActionByName = actor?.FullName,
                        ActionAt = DateTime.Now
                    });
                }
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "تم حفظ تقييم الأثر";
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> CanAccessAsync(TrainingImpactAssessment assessment)
        {
            if (IsPrivileged) return true;
            var actor = await _userManager.GetUserAsync(User);
            return actor?.Department != null &&
                   assessment.TrainingNeed != null &&
                   assessment.TrainingNeed.Department == actor.Department;
        }
    }
}
