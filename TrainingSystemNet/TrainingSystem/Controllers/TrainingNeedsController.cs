using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using TrainingSystem.Data;
using TrainingSystem.Models;
using TrainingSystem.Services;

namespace TrainingSystem.Controllers
{
    // إدارة الاحتياجات التدريبية للموظفين حسب الدائرة (مصدر البيانات: قاعدة البيانات).
    // المشرف يرى ويدير دائرته فقط، والمدير/مدير النظام يديرون كل الدوائر.
    [Authorize(Roles = "SuperAdmin,Admin,Supervisor")]
    public class TrainingNeedsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TrainingNeedsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private bool IsPrivileged =>
            User.IsInRole(SystemRoles.SuperAdmin) || User.IsInRole(SystemRoles.Admin);

        // ==================== العرض ====================

        public async Task<IActionResult> Index(string? department, string? search)
        {
            var actor = await _userManager.GetUserAsync(User);
            var myDept = actor?.Department;

            var query = _context.TrainingNeeds.AsQueryable();

            // تقييد المشرف بدائرته فقط (حماية من الوصول غير المصرّح لبيانات دوائر أخرى)
            if (!IsPrivileged)
            {
                query = query.Where(n => n.Department == myDept);
            }
            else if (!string.IsNullOrWhiteSpace(department))
            {
                query = query.Where(n => n.Department == department);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                query = query.Where(n =>
                    n.EmployeeName.Contains(s) ||
                    (n.EmployeeNumber != null && n.EmployeeNumber.Contains(s)) ||
                    n.SkillName.Contains(s));
            }

            var needs = await query
                .OrderBy(n => n.Department)
                .ThenBy(n => n.EmployeeName)
                .ThenByDescending(n => n.Priority)
                .ToListAsync();

            var totalRequired = needs.Sum(n => n.RequiredLevel);
            var totalCurrent = needs.Sum(n => Math.Min(n.CurrentLevel, n.RequiredLevel));

            // اقتراح البرنامج التدريبي الأنسب لكل احتياج من البرامج النشطة (قراءة فقط، دون أي تعديل على القاعدة)
            var activePrograms = await _context.TrainingPrograms
                .Where(p => p.Status == ProgramStatus.Active)
                .Select(p => new { p.Title, p.Categories })
                .ToListAsync();

            var suggestions = new Dictionary<int, ProgramSuggestion>();
            foreach (var n in needs)
            {
                suggestions[n.Id] = SuggestProgram(n, activePrograms.Select(p => (p.Title, p.Categories)).ToList());
            }

            // متوسط الفجوة لكل دائرة (لرسم الأعمدة)
            var gapByDept = needs
                .GroupBy(n => n.Department)
                .Select(g => new DepartmentGap
                {
                    Department = g.Key,
                    AverageGap = Math.Round(g.Average(x => (double)Math.Max(0, x.RequiredLevel - x.CurrentLevel)), 1)
                })
                .OrderByDescending(d => d.AverageGap)
                .ToList();

            var vm = new TrainingNeedsIndexViewModel
            {
                Needs = needs,
                TotalNeeds = needs.Count,
                EmployeeCount = needs
                    .Select(n => (n.EmployeeNumber ?? "") + "|" + n.EmployeeName)
                    .Distinct().Count(),
                HighPriorityCount = needs.Count(n => n.Priority == TrainingNeedPriority.High || n.Priority == TrainingNeedPriority.Critical),
                InProgressCount = needs.Count(n => n.Status == TrainingNeedStatus.InProgress),
                Readiness = totalRequired > 0
                    ? (int)Math.Round(100.0 * totalCurrent / totalRequired)
                    : 0,
                IsPrivileged = IsPrivileged,
                CurrentDepartment = IsPrivileged ? department : myDept,
                Search = search,
                Departments = await GetDepartmentsAsync(),
                Suggestions = suggestions,
                GapByDepartment = gapByDept
            };

            return View(vm);
        }

        // ==================== إضافة يدوية ====================

        public async Task<IActionResult> Create()
        {
            var vm = new TrainingNeedFormViewModel { RequiredLevel = 3, CurrentLevel = 1 };
            await PopulateFormOptionsAsync(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TrainingNeedFormViewModel model)
        {
            var actor = await _userManager.GetUserAsync(User);
            var myDept = actor?.Department;

            // إذا تم اختيار موظف من النظام، تُشتقّ بياناته من الخادم (مصدر موثوق)
            ApplicationUser? employee = null;
            if (!string.IsNullOrWhiteSpace(model.EmployeeUserId))
            {
                employee = await _userManager.FindByIdAsync(model.EmployeeUserId);
                if (employee != null)
                {
                    model.EmployeeName = employee.FullName;
                    model.EmployeeNumber = employee.EmployeeNumber ?? employee.UserName;
                    model.Department = employee.Department;
                    model.JobTitle = employee.JobTitle;
                    model.Grade = employee.Grade;
                }
            }

            // إذا اختير المهارة من القائمة، يُشتقّ اسمها وتصنيفها من الخادم
            if (model.SkillId.HasValue)
            {
                var skill = await _context.Skills
                    .Include(s => s.SkillCategory)
                    .FirstOrDefaultAsync(s => s.Id == model.SkillId.Value);
                if (skill != null)
                {
                    model.SkillName = skill.Name;
                    model.Category = skill.SkillCategory?.Name ?? model.Category;
                }
            }

            // المشرف: تُفرض دائرته دائماً بغضّ النظر عن أي قيمة واردة (منع IDOR / تلاعب)
            if (!IsPrivileged)
            {
                model.Department = myDept;
            }

            if (string.IsNullOrWhiteSpace(model.EmployeeName))
                ModelState.AddModelError(nameof(model.EmployeeName), "اسم الموظف مطلوب");
            if (string.IsNullOrWhiteSpace(model.SkillName))
                ModelState.AddModelError(nameof(model.SkillName), "اسم المهارة مطلوب");
            if (string.IsNullOrWhiteSpace(model.Department))
                ModelState.AddModelError(nameof(model.Department), "الدائرة مطلوبة");
            if (model.RequiredLevel < 1 || model.RequiredLevel > 5)
                ModelState.AddModelError(nameof(model.RequiredLevel), "المستوى المطلوب بين 1 و 5");
            if (model.CurrentLevel < 0 || model.CurrentLevel > 5)
                ModelState.AddModelError(nameof(model.CurrentLevel), "المستوى الحالي بين 0 و 5");

            // المشرف لا يضيف احتياجاً لموظف خا�������������������� دائرته
            if (!IsPrivileged && employee != null && employee.Department != myDept)
                ModelState.AddModelError(string.Empty, "لا يمكنك إضافة احتياج لموظف خارج دائرتك");

            if (!ModelState.IsValid)
            {
                await PopulateFormOptionsAsync(model);
                return View(model);
            }

            var submitting = model.SubmitForApproval;
            var need = new TrainingNeed
            {
                EmployeeName = model.EmployeeName!.Trim(),
                EmployeeNumber = model.EmployeeNumber?.Trim(),
                EmployeeUserId = model.EmployeeUserId,
                Department = model.Department!.Trim(),
                SkillName = model.SkillName!.Trim(),
                SkillId = model.SkillId,
                SkillCategoryId = model.SkillCategoryId,
                Category = model.Category?.Trim(),
                JobTitle = model.JobTitle?.Trim(),
                Grade = model.Grade?.Trim(),
                TrainingReason = model.TrainingReason,
                FailedGoalNumber = model.TrainingReason == Models.TrainingReason.PerformanceWeakness ? model.FailedGoalNumber?.Trim() : null,
                PerformanceGapDescription = model.TrainingReason == Models.TrainingReason.PerformanceWeakness ? model.PerformanceGapDescription?.Trim() : null,
                Justification = model.Justification?.Trim(),
                NeedDescription = model.NeedDescription?.Trim(),
                GapType = model.GapType,
                ProposedTrainingProgram = model.ProposedTrainingProgram?.Trim(),
                ProposedProgramDescription = model.ProposedProgramDescription?.Trim(),
                PreferredTrainingProvider = model.PreferredTrainingProvider?.Trim(),
                TrainingMode = model.TrainingMode,
                ProposedDuration = model.ProposedDuration?.Trim(),
                SuggestedTimeframe = model.SuggestedTimeframe?.Trim(),
                RequiredLevel = model.RequiredLevel,
                CurrentLevel = model.CurrentLevel,
                GapScore = Math.Max(0, model.RequiredLevel - model.CurrentLevel),
                Priority = TrainingNeed.ClassifyByScore(TrainingNeed.ComputePriorityScore(
                    model.RequiredLevel, model.CurrentLevel, model.ImpactScore, model.RiskScore, model.IsCompliance)),
                PriorityScore = TrainingNeed.ComputePriorityScore(
                    model.RequiredLevel, model.CurrentLevel, model.ImpactScore, model.RiskScore, model.IsCompliance),
                Status = TrainingNeedStatus.New,
                ApprovalStatus = submitting ? TrainingNeedApprovalStatus.SubmittedToManager : TrainingNeedApprovalStatus.Draft,
                SubmittedAt = submitting ? DateTime.Now : (DateTime?)null,
                Notes = model.Notes?.Trim(),
                LinkedTrainingProgramId = model.LinkedTrainingProgramId,
                TrainingBatchId = model.TrainingBatchId,
                PlannedCost = Math.Max(0, model.PlannedCost),
                ActualCost = Math.Max(0, model.ActualCost),
                PlannedDate = model.PlannedDate,
                CompletionDate = model.CompletionDate,
                EstimatedCost = Math.Max(0, model.EstimatedCost),
                ParticipantsCount = Math.Max(1, model.ParticipantsCount),
                ImpactScore = Math.Clamp(model.ImpactScore, 1, 4),
                RiskScore = Math.Clamp(model.RiskScore, 1, 4),
                IsCompliance = model.IsCompliance,
                CreatedByUserId = actor?.Id,
                CreatedAt = DateTime.Now
            };

            // إن اختير برنامج ودفعة، تأكد أن الدفعة تابعة لنفس البرنامج
            await AlignBatchToProgramAsync(need);
            await AlignSkillCategoryAsync(need);

            _context.TrainingNeeds.Add(need);
            await _context.SaveChangesAsync();

            AddHistory(need, need.ApprovalStatus,
                submitting ? "إنشاء ��إرسال للاعتماد" : "إنشاء كمسودة", null, actor);

            if (submitting)
            {
                var managerIds = await GetDepartmentManagerIdsAsync(need.Department);
                await NotificationHelper.AddToRoleAsync(_context, managerIds,
                    $"احتياج تدريبي جديد بانتظار اعتمادك: {need.SkillName} — {need.EmployeeName}",
                    NotificationType.Info, need.Id);
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = submitting
                ? "تم إنشاء الاحتياج وإرساله لاعتماد المدير"
                : "تم حفظ الاحتياج التدريبي كمسودة";
            return RedirectToAction(nameof(Index));
        }

        // ==================== تعديل ====================

        public async Task<IActionResult> Edit(int id)
        {
            var need = await _context.TrainingNeeds.FindAsync(id);
            if (need == null) return NotFound();
            if (!await CanAccessAsync(need))
            {
                TempData["Error"] = "لا تملك صلاحية الوصول لهذا السجل";
                return RedirectToAction(nameof(Index));
            }

            var vm = new TrainingNeedFormViewModel
            {
                Id = need.Id,
                EmployeeUserId = need.EmployeeUserId,
                EmployeeName = need.EmployeeName,
                EmployeeNumber = need.EmployeeNumber,
                Department = need.Department,
                SkillName = need.SkillName,
                SkillId = need.SkillId,
                SkillCategoryId = need.SkillCategoryId,
                Category = need.Category,
                JobTitle = need.JobTitle,
                Grade = need.Grade,
                TrainingReason = need.TrainingReason,
                FailedGoalNumber = need.FailedGoalNumber,
                PerformanceGapDescription = need.PerformanceGapDescription,
                Justification = need.Justification,
                NeedDescription = need.NeedDescription,
                GapType = need.GapType,
                ProposedTrainingProgram = need.ProposedTrainingProgram,
                ProposedProgramDescription = need.ProposedProgramDescription,
                PreferredTrainingProvider = need.PreferredTrainingProvider,
                TrainingMode = need.TrainingMode,
                ProposedDuration = need.ProposedDuration,
                SuggestedTimeframe = need.SuggestedTimeframe,
                RequiredLevel = need.RequiredLevel,
                CurrentLevel = need.CurrentLevel,
                Status = need.Status,
                Notes = need.Notes,
                LinkedTrainingProgramId = need.LinkedTrainingProgramId,
                TrainingBatchId = need.TrainingBatchId,
                PlannedCost = need.PlannedCost,
                ActualCost = need.ActualCost,
                PlannedDate = need.PlannedDate,
                CompletionDate = need.CompletionDate,
                EstimatedCost = need.EstimatedCost,
                ParticipantsCount = need.ParticipantsCount,
                ImpactScore = need.ImpactScore,
                RiskScore = need.RiskScore,
                IsCompliance = need.IsCompliance
            };
            await PopulateFormOptionsAsync(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TrainingNeedFormViewModel model)
        {
            if (id != model.Id) return NotFound();

            var need = await _context.TrainingNeeds.FindAsync(id);
            if (need == null) return NotFound();
            if (!await CanAccessAsync(need))
            {
                TempData["Error"] = "لا تملك صلاحية الوصول لهذا السجل";
                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrWhiteSpace(model.EmployeeName))
                ModelState.AddModelError(nameof(model.EmployeeName), "اسم الموظف مطلوب");
            if (string.IsNullOrWhiteSpace(model.SkillName))
                ModelState.AddModelError(nameof(model.SkillName), "اسم المهارة مطلوب");
            if (model.RequiredLevel < 1 || model.RequiredLevel > 5)
                ModelState.AddModelError(nameof(model.RequiredLevel), "المستوى المطلوب بين 1 و 5");
            if (model.CurrentLevel < 0 || model.CurrentLevel > 5)
                ModelState.AddModelError(nameof(model.CurrentLevel), "المستوى الحالي بين 0 و 5");

            if (!ModelState.IsValid)
            {
                await PopulateFormOptionsAsync(model);
                return View(model);
            }

            // إن اختير مهارة من القائمة، يُشتقّ اسمها/تصنيفها من الخادم
            if (model.SkillId.HasValue)
            {
                var skill = await _context.Skills
                    .Include(s => s.SkillCategory)
                    .FirstOrDefaultAsync(s => s.Id == model.SkillId.Value);
                if (skill != null)
                {
                    model.SkillName = skill.Name;
                    model.Category = skill.SkillCategory?.Name ?? model.Category;
                }
            }

            // إن اختير موظف من النظام، تُشتقّ بياناته من الخادم (مصدر موثوق — لا تُكرَّر يدوياً)
            if (!string.IsNullOrWhiteSpace(model.EmployeeUserId))
            {
                var employee = await _userManager.FindByIdAsync(model.EmployeeUserId);
                if (employee != null)
                {
                    model.EmployeeName = employee.FullName;
                    model.EmployeeNumber = employee.EmployeeNumber ?? employee.UserName;
                    model.JobTitle = employee.JobTitle;
                    model.Grade = employee.Grade;
                    need.EmployeeUserId = employee.Id;
                }
            }
            else
            {
                need.EmployeeUserId = null;
            }

            // الدائرة لا تتغيّر عبر التعديل (تبقى كما هي لضمان بقاء السجل ضمن نطاق الدائرة)
            need.EmployeeName = model.EmployeeName!.Trim();
            need.EmployeeNumber = model.EmployeeNumber?.Trim();
            need.SkillName = model.SkillName!.Trim();
            need.SkillId = model.SkillId;
            need.SkillCategoryId = model.SkillCategoryId;
            need.Category = model.Category?.Trim();
            need.JobTitle = model.JobTitle?.Trim();
            need.Grade = model.Grade?.Trim();
            need.TrainingReason = model.TrainingReason;
            need.FailedGoalNumber = model.TrainingReason == Models.TrainingReason.PerformanceWeakness ? model.FailedGoalNumber?.Trim() : null;
            need.PerformanceGapDescription = model.TrainingReason == Models.TrainingReason.PerformanceWeakness ? model.PerformanceGapDescription?.Trim() : null;
            need.Justification = model.Justification?.Trim();
            need.NeedDescription = model.NeedDescription?.Trim();
            need.GapType = model.GapType;
            need.ProposedTrainingProgram = model.ProposedTrainingProgram?.Trim();
            need.ProposedProgramDescription = model.ProposedProgramDescription?.Trim();
            need.PreferredTrainingProvider = model.PreferredTrainingProvider?.Trim();
            need.TrainingMode = model.TrainingMode;
            need.ProposedDuration = model.ProposedDuration?.Trim();
            need.SuggestedTimeframe = model.SuggestedTimeframe?.Trim();
            need.RequiredLevel = model.RequiredLevel;
            need.CurrentLevel = model.CurrentLevel;
            need.GapScore = Math.Max(0, model.RequiredLevel - model.CurrentLevel);
            need.LinkedTrainingProgramId = model.LinkedTrainingProgramId;
            need.TrainingBatchId = model.TrainingBatchId;
            need.PlannedCost = Math.Max(0, model.PlannedCost);
            need.ActualCost = Math.Max(0, model.ActualCost);
            need.PlannedDate = model.PlannedDate;
            need.CompletionDate = model.CompletionDate;
            await AlignBatchToProgramAsync(need);
            await AlignSkillCategoryAsync(need);
            need.EstimatedCost = Math.Max(0, model.EstimatedCost);
            need.ParticipantsCount = Math.Max(1, model.ParticipantsCount);
            need.ImpactScore = Math.Clamp(model.ImpactScore, 1, 4);
            need.RiskScore = Math.Clamp(model.RiskScore, 1, 4);
            need.IsCompliance = model.IsCompliance;
            need.PriorityScore = TrainingNeed.ComputePriorityScore(
                model.RequiredLevel, model.CurrentLevel, need.ImpactScore, need.RiskScore, need.IsCompliance);
            need.Priority = TrainingNeed.ClassifyByScore(need.PriorityScore);
            need.Status = model.Status;
            need.Notes = model.Notes?.Trim();
            need.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            TempData["Success"] = "تم تحديث الاحتياج التدريب�� بنجاح";
            return RedirectToAction(nameof(Index));
        }

        // ==================== حذف ====================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var need = await _context.TrainingNeeds.FindAsync(id);
            if (need == null) return RedirectToAction(nameof(Index));
            if (!await CanAccessAsync(need))
            {
                TempData["Error"] = "لا تملك صلاحية الوصول لهذا السجل";
                return RedirectToAction(nameof(Index));
            }

            _context.TrainingNeeds.Remove(need);
            await _context.SaveChangesAsync();
            TempData["Success"] = "تم حذف الاحتياج التدريبي";
            return RedirectToAction(nameof(Index));
        }

        // ==================== التفاصيل ====================

        public async Task<IActionResult> Details(int id)
        {
            var need = await _context.TrainingNeeds
                .Include(n => n.StatusHistory)
                .Include(n => n.LinkedTrainingProgram)
                .Include(n => n.TrainingBatch)
                    .ThenInclude(b => b!.TrainingProgram)
                .Include(n => n.ImpactAssessments)
                .Include(n => n.Employee)
                .Include(n => n.SkillCategoryRef)
                .FirstOrDefaultAsync(n => n.Id == id);
            if (need == null) return NotFound();
            if (!await CanAccessAsync(need))
            {
                TempData["Error"] = "لا تملك صلاحية الوصول لهذا السجل";
                return RedirectToAction(nameof(Index));
            }

            // اسم المدير المباشر للموظف المربوط (عبر FK — دون تكرار)
            if (!string.IsNullOrWhiteSpace(need.Employee?.ManagerUserId))
            {
                ViewBag.ManagerName = await _context.Users
                    .AsNoTracking()
                    .Where(u => u.Id == need.Employee.ManagerUserId)
                    .Select(u => u.FullName)
                    .FirstOrDefaultAsync();
            }

            // حالة الميزانية المركزية للسنة المالية الحالية (للعرض ومقارنة تكلفة الطلب)
            ViewBag.CurrentBudget = await GetOrmCurrentBudgetAsync();
            ViewBag.NeedBudgetCost = GetNeedBudgetCost(need);
            return View(need);
        }

        // ==================== سير الاعتماد ====================

        // إرسال مسودة للاعتماد (المدير المباشر)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(int id)
        {
            var need = await _context.TrainingNeeds.FindAsync(id);
            if (need == null) return NotFound();
            if (!await CanAccessAsync(need))
            {
                TempData["Error"] = "لا تملك صلاحية الوصول لهذا السجل";
                return RedirectToAction(nameof(Index));
            }
            if (need.ApprovalStatus != TrainingNeedApprovalStatus.Draft &&
                need.ApprovalStatus != TrainingNeedApprovalStatus.ReturnedForModification &&
                need.ApprovalStatus != TrainingNeedApprovalStatus.ManagerRejected &&
                need.ApprovalStatus != TrainingNeedApprovalStatus.HRRejected)
            {
                TempData["Error"] = "لا يمكن إرسال هذا الاحتياج في حالته الحالية";
                return RedirectToAction(nameof(Details), new { id });
            }

            var actor = await _userManager.GetUserAsync(User);
            need.ApprovalStatus = TrainingNeedApprovalStatus.SubmittedToManager;
            need.SubmittedAt = DateTime.Now;
            AddHistory(need, need.ApprovalStatus, "إرسال للاعتماد", null, actor);

            // إشعار المدراء المسؤولين عن دائرة الاحتياج
            var managerIds = await GetDepartmentManagerIdsAsync(need.Department);
            await NotificationHelper.AddToRoleAsync(_context, managerIds,
                $"احتياج تدريبي جديد بانتظار اعتمادك: {need.SkillName} — {need.EmployeeName}",
                NotificationType.Info, need.Id);

            await _context.SaveChangesAsync();

            TempData["Success"] = "تم إرسال الاحتياج لاعتماد المدير المباشر";
            return RedirectToAction(nameof(Details), new { id });
        }

        // اعتماد المدير المباشر (يُدار من المشرف/المدير)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManagerApprove(int id, string? comment)
        {
            var need = await _context.TrainingNeeds.FindAsync(id);
            if (need == null) return NotFound();
            if (!await CanAccessAsync(need))
            {
                TempData["Error"] = "لا تملك صلاحية الوصول لهذا السجل";
                return RedirectToAction(nameof(Index));
            }
            if (need.ApprovalStatus != TrainingNeedApprovalStatus.SubmittedToManager)
            {
                TempData["Error"] = "هذا الاحتياج ليس بانتظار اعتماد المدير";
                return RedirectToAction(nameof(Details), new { id });
            }

            var actor = await _userManager.GetUserAsync(User);
            need.ApprovalStatus = TrainingNeedApprovalStatus.ManagerApproved;
            need.ManagerUserId = actor?.Id;
            need.ManagerActionAt = DateTime.Now;
            need.ManagerComment = comment?.Trim();
            AddHistory(need, need.ApprovalStatus, "اعتماد المدير المباشر", comment, actor);

            // إشعار دائرة التدريب + مقدّم الطلب
            var hrIds = await GetHRUserIdsAsync();
            await NotificationHelper.AddToRoleAsync(_context, hrIds,
                $"احتياج معتمد من المدير بانتظار مراجعة دائرة التدريب: {need.SkillName} — {need.Department}",
                NotificationType.Info, need.Id);
            if (!string.IsNullOrEmpty(need.CreatedByUserId))
                NotificationHelper.Add(_context, need.CreatedByUserId,
                    $"تم اعتماد احتياجك من المدير المباشر: {need.SkillName}",
                    NotificationType.Success, need.Id);

            await _context.SaveChangesAsync();

            TempData["Success"] = "تم اعتماد الاحتياج وإحالته للموارد البشرية";
            return RedirectToAction(nameof(Details), new { id });
        }

        // رفض المدير المباشر
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManagerReject(int id, string? comment)
        {
            var need = await _context.TrainingNeeds.FindAsync(id);
            if (need == null) return NotFound();
            if (!await CanAccessAsync(need))
            {
                TempData["Error"] = "لا تملك صلاحية الوصول لهذا السجل";
                return RedirectToAction(nameof(Index));
            }
            if (need.ApprovalStatus != TrainingNeedApprovalStatus.SubmittedToManager)
            {
                TempData["Error"] = "هذا الاحتياج ليس بانتظار اعتماد المدير";
                return RedirectToAction(nameof(Details), new { id });
            }

            var actor = await _userManager.GetUserAsync(User);
            need.ApprovalStatus = TrainingNeedApprovalStatus.ManagerRejected;
            need.ManagerUserId = actor?.Id;
            need.ManagerActionAt = DateTime.Now;
            need.ManagerComment = comment?.Trim();
            AddHistory(need, need.ApprovalStatus, "رفض المدير المباشر", comment, actor);

            if (!string.IsNullOrEmpty(need.CreatedByUserId))
                NotificationHelper.Add(_context, need.CreatedByUserId,
                    $"تم رفض احتياجك من المدير المباشر: {need.SkillName}" +
                    (string.IsNullOrWhiteSpace(comment) ? "" : $" — {comment.Trim()}"),
                    NotificationType.Danger, need.Id);

            await _context.SaveChangesAsync();

            TempData["Success"] = "تم رفض الاحتياج";
            return RedirectToAction(nameof(Details), new { id });
        }

        // طلب تعديل من المدير المباشر — يعيد الطلب لمقدّمه كمسودة مع بيان المطلوب تعديله
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManagerRequestChanges(int id, string? comment)
        {
            var need = await _context.TrainingNeeds.FindAsync(id);
            if (need == null) return NotFound();
            if (!await CanAccessAsync(need))
            {
                TempData["Error"] = "لا تملك صلاحية الوصول لهذا السجل";
                return RedirectToAction(nameof(Index));
            }
            if (need.ApprovalStatus != TrainingNeedApprovalStatus.SubmittedToManager)
            {
                TempData["Error"] = "هذا الاحتياج ليس بانتظار اعتماد المدير";
                return RedirectToAction(nameof(Details), new { id });
            }
            if (string.IsNullOrWhiteSpace(comment))
            {
                TempData["Error"] = "يرجى بيان التعديل المطلوب";
                return RedirectToAction(nameof(Details), new { id });
            }

            var actor = await _userManager.GetUserAsync(User);
            need.ApprovalStatus = TrainingNeedApprovalStatus.ReturnedForModification;
            need.ManagerUserId = actor?.Id;
            need.ManagerActionAt = DateTime.Now;
            need.ManagerComment = comment.Trim();
            AddHistory(need, need.ApprovalStatus, "طلب تعديل من ال��دي�� المباشر", comment, actor);

            if (!string.IsNullOrEmpty(need.CreatedByUserId))
                NotificationHelper.Add(_context, need.CreatedByUserId,
                    $"طلب المدير تعديل احتياجك: {need.SkillName} — {comment.Trim()}",
                    NotificationType.Warning, need.Id);

            await _context.SaveChangesAsync();

            TempData["Success"] = "تمت إعادة الاحتياج لمقدّمه لطلب تعديل";
            return RedirectToAction(nameof(Details), new { id });
        }

        // اعتماد دائرة التدريب (المدير/مدير النظام فقط)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> HRApprove(int id, string? comment)
        {
            var need = await _context.TrainingNeeds.FindAsync(id);
            if (need == null) return NotFound();
            if (need.ApprovalStatus != TrainingNeedApprovalStatus.ManagerApproved)
            {
                TempData["Error"] = "هذا الاحتياج ليس بانتظار اعتماد دائرة التدريب";
                return RedirectToAction(nameof(Details), new { id });
            }

            var actor = await _userManager.GetUserAsync(User);
            var cost = GetNeedBudgetCost(need);
            var budget = await GetOrmCurrentBudgetAsync();

            // فحص الميزانية: إن تجاوزت التكلفة التقديرية المتبقي يُنقل الطلب لمراجعة الميزانية بدل الاعتماد المباشر
            if (budget != null && cost > 0 && cost > budget.RemainingBudget)
            {
                need.ApprovalStatus = TrainingNeedApprovalStatus.BudgetReview;
                need.HRUserId = actor?.Id;
                need.HRActionAt = DateTime.Now;
                need.HRComment = comment?.Trim();
                AddHistory(need, need.ApprovalStatus,
                    $"تجاوز التكلفة التقديرية ({cost:N0}) المتبقي من الميزانية ({budget.RemainingBudget:N0}) — يتطلب مراجعة الميزانية",
                    comment, actor);

                foreach (var hrId in await GetHRUserIdsAsync())
                    NotificationHelper.Add(_context, hrId,
                        $"احتياج يتجاوز الميزانية المتاحة ويتطلب مراجعة: {need.SkillName}",
                        NotificationType.Warning, need.Id);

                await _context.SaveChangesAsync();
                TempData["Error"] = $"التكلفة التقديرية ({cost:N0}) تتجاوز المتبقي من الميزانية ({budget.RemainingBudget:N0}). تم نقل الطلب إلى مراجعة الميزانية.";
                return RedirectToAction(nameof(Details), new { id });
            }

            need.ApprovalStatus = TrainingNeedApprovalStatus.HRApproved;
            need.HRUserId = actor?.Id;
            need.HRActionAt = DateTime.Now;
            need.ApprovalDate = DateTime.Now;
            need.HRComment = comment?.Trim();
            need.Status = TrainingNeedStatus.InProgress;

            // ضمن الميزانية → التزام المبلغ التقديري على الموازنة المركزية
            if (budget != null && cost > 0)
            {
                budget.CommittedBudget += cost;
                budget.UpdatedAt = DateTime.Now;
            }

            AddHistory(need, need.ApprovalStatus, "اعتماد دائرة التدريب (نهائي)", comment, actor);

            if (!string.IsNullOrEmpty(need.CreatedByUserId))
                NotificationHelper.Add(_context, need.CreatedByUserId,
                    $"تم اعتماد احتياجك نهائياً من دائرة التدريب: {need.SkillName}",
                    NotificationType.Success, need.Id);

            await _context.SaveChangesAsync();

            TempData["Success"] = "تم اعتماد الاحتياج نهائياً";
            return RedirectToAction(nameof(Details), new { id });
        }

        // اعتماد نهائي رغم تجاوز الميزانية (تجاوز مخوّل) — من مرحلة مراجعة الميزانية
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> BudgetReviewApprove(int id, string? comment)
        {
            var need = await _context.TrainingNeeds.FindAsync(id);
            if (need == null) return NotFound();
            if (need.ApprovalStatus != TrainingNeedApprovalStatus.BudgetReview)
            {
                TempData["Error"] = "هذا الاحتياج ليس في مرحلة مراجعة الميزانية";
                return RedirectToAction(nameof(Details), new { id });
            }

            var actor = await _userManager.GetUserAsync(User);
            var cost = GetNeedBudgetCost(need);
            var budget = await GetOrmCurrentBudgetAsync();

            need.ApprovalStatus = TrainingNeedApprovalStatus.HRApproved;
            need.HRUserId = actor?.Id;
            need.HRActionAt = DateTime.Now;
            need.ApprovalDate = DateTime.Now;
            need.HRComment = comment?.Trim();
            need.Status = TrainingNeedStatus.InProgress;

            if (budget != null && cost > 0)
            {
                budget.CommittedBudget += cost; // قد يتجاوز المخصص (متابعة فقط)
                budget.UpdatedAt = DateTime.Now;
            }

            AddHistory(need, need.ApprovalStatus, "اعتماد نهائي رغم تجاوز الميزانية (تجاوز مخوّل)", comment, actor);

            if (!string.IsNullOrEmpty(need.CreatedByUserId))
                NotificationHelper.Add(_context, need.CreatedByUserId,
                    $"تم اعتماد احتياجك نهائياً: {need.SkillName}", NotificationType.Success, need.Id);

            await _context.SaveChangesAsync();
            TempData["Success"] = "تم الاعتماد النهائي رغم تجاوز الميزانية";
            return RedirectToAction(nameof(Details), new { id });
        }

        // رفض الطلب في مرحلة مراجعة الميزانية
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> BudgetReviewReject(int id, string? comment)
        {
            var need = await _context.TrainingNeeds.FindAsync(id);
            if (need == null) return NotFound();
            if (need.ApprovalStatus != TrainingNeedApprovalStatus.BudgetReview)
            {
                TempData["Error"] = "هذا الاحتياج ليس في مرحلة مراجعة الميزانية";
                return RedirectToAction(nameof(Details), new { id });
            }

            var actor = await _userManager.GetUserAsync(User);
            need.ApprovalStatus = TrainingNeedApprovalStatus.HRRejected;
            need.HRUserId = actor?.Id;
            need.HRActionAt = DateTime.Now;
            need.HRComment = comment?.Trim();
            AddHistory(need, need.ApprovalStatus, "رفض لتجاوز الميزانية", comment, actor);

            if (!string.IsNullOrEmpty(need.CreatedByUserId))
                NotificationHelper.Add(_context, need.CreatedByUserId,
                    $"تم رفض احتياجك لتجاوز الميزانية: {need.SkillName}", NotificationType.Danger, need.Id);

            await _context.SaveChangesAsync();
            TempData["Success"] = "تم رفض الاحتياج";
            return RedirectToAction(nameof(Details), new { id });
        }

        // رفض دائرة التدريب (المدير/مدير النظام فقط)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> HRReject(int id, string? comment)
        {
            var need = await _context.TrainingNeeds.FindAsync(id);
            if (need == null) return NotFound();
            if (need.ApprovalStatus != TrainingNeedApprovalStatus.ManagerApproved)
            {
                TempData["Error"] = "هذا الاحتياج ليس بانتظار اعتماد دائرة التدريب";
                return RedirectToAction(nameof(Details), new { id });
            }

            var actor = await _userManager.GetUserAsync(User);
            need.ApprovalStatus = TrainingNeedApprovalStatus.HRRejected;
            need.HRUserId = actor?.Id;
            need.HRActionAt = DateTime.Now;
            need.HRComment = comment?.Trim();
            AddHistory(need, need.ApprovalStatus, "رفض دائرة التدريب", comment, actor);

            if (!string.IsNullOrEmpty(need.CreatedByUserId))
                NotificationHelper.Add(_context, need.CreatedByUserId,
                    $"تم رفض احتياجك من دائرة التدريب: {need.SkillName}",
                    NotificationType.Danger, need.Id);

            await _context.SaveChangesAsync();

            TempData["Success"] = "تم رفض الاحتياج";
            return RedirectToAction(nameof(Details), new { id });
        }

        // جدولة التدريب (بعد الاعتماد النهائي) — ال��وارد البشرية
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> ScheduleTraining(int id)
        {
            var need = await _context.TrainingNeeds.FindAsync(id);
            if (need == null) return NotFound();
            if (need.ApprovalStatus != TrainingNeedApprovalStatus.HRApproved)
            {
                TempData["Error"] = "يجب اعتماد الاحتياج نهائياً قبل جدولته";
                return RedirectToAction(nameof(Details), new { id });
            }

            var actor = await _userManager.GetUserAsync(User);
            need.ApprovalStatus = TrainingNeedApprovalStatus.TrainingScheduled;
            AddHistory(need, need.ApprovalStatus, "جدولة التدريب وتسجيل الموظف", null, actor);

            if (!string.IsNullOrEmpty(need.CreatedByUserId))
                NotificationHelper.Add(_context, need.CreatedByUserId,
                    $"تمت جدولة تدريب: {need.SkillName}", NotificationType.Info, need.Id);
            if (!string.IsNullOrEmpty(need.EmployeeUserId))
                NotificationHelper.Add(_context, need.EmployeeUserId,
                    $"تم تسجيلك في تدريب: {need.SkillName}", NotificationType.Info, need.Id);

            await _context.SaveChangesAsync();
            TempData["Success"] = "تمت جدولة التدريب وتسجيل الموظف";
            return RedirectToAction(nameof(Details), new { id });
        }

        // إنهاء التدريب — الموازنة وتقييم الأثر مؤجّلان في المرحلة الحالية
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> CompleteTraining(int id)
        {
            var need = await _context.TrainingNeeds
                .Include(n => n.Employee)
                .FirstOrDefaultAsync(n => n.Id == id);
            if (need == null) return NotFound();
            if (need.ApprovalStatus != TrainingNeedApprovalStatus.TrainingScheduled)
            {
                TempData["Error"] = "لا يمكن إنهاء تدريب غير مجدول";
                return RedirectToAction(nameof(Details), new { id });
            }

            var actor = await _userManager.GetUserAsync(User);
            need.Status = TrainingNeedStatus.Completed;

            // تسوية الميزانية: نقل المبلغ الملتزم به إلى المصروف الفعلي
            var committed = GetNeedBudgetCost(need);
            if (committed > 0)
            {
                var budget = await GetOrmCurrentBudgetAsync();
                if (budget != null)
                {
                    var actual = need.ActualCost > 0 ? need.ActualCost : committed;
                    budget.CommittedBudget = Math.Max(0, budget.CommittedBudget - committed);
                    budget.ActualSpending += actual;
                    budget.UpdatedAt = DateTime.Now;
                }
            }

            // إنشاء تقييمي الأثر تلقائياً: مباشر بعد التدريب + بعد 90 يوماً (إن لم يوجدا)
            var now = DateTime.Now;
            var existingTypes = await _context.TrainingImpactAssessments
                .Where(a => a.TrainingNeedId == need.Id)
                .Select(a => a.AssessmentType)
                .ToListAsync();

            if (!existingTypes.Contains(ImpactAssessmentType.PostTraining))
            {
                _context.TrainingImpactAssessments.Add(new TrainingImpactAssessment
                {
                    TrainingNeedId = need.Id,
                    AssessmentType = ImpactAssessmentType.PostTraining,
                    DueDate = now,
                    CreatedAt = now
                });
            }
            if (!existingTypes.Contains(ImpactAssessmentType.Day90))
            {
                _context.TrainingImpactAssessments.Add(new TrainingImpactAssessment
                {
                    TrainingNeedId = need.Id,
                    AssessmentType = ImpactAssessmentType.Day90,
                    DueDate = now.AddDays(90),
                    CreatedAt = now
                });
            }

            // الطلب ينتقل إلى مرحلة تقييم الأثر
            need.ApprovalStatus = TrainingNeedApprovalStatus.ImpactAssessmentPending;
            AddHistory(need, need.ApprovalStatus, "إنهاء التدريب وإنشاء تقييمي الأثر (مباشر + بعد 90 يوماً)", null, actor);

            if (!string.IsNullOrEmpty(need.CreatedByUserId))
                NotificationHelper.Add(_context, need.CreatedByUserId,
                    $"اكتمل تدريب: {need.SkillName} — يتطلب تقييم الأثر", NotificationType.Success, need.Id);

            // إشعار مسؤول التقييم (المدير المباشر إن وُجد، وإلا مقدّم الطلب)
            var assessorId = need.Employee?.ManagerUserId ?? need.CreatedByUserId;
            if (!string.IsNullOrEmpty(assessorId))
                NotificationHelper.Add(_context, assessorId,
                    $"يلزم تعبئة تقييم الأثر المباشر لتدريب: {need.SkillName}", NotificationType.Warning, need.Id);

            await _context.SaveChangesAsync();
            TempData["Success"] = "تم إنهاء التدريب وإنشاء تقييمي الأثر تلقائياً";
            return RedirectToAction(nameof(Details), new { id });
        }

        // إغلاق الطلب نهائياً بعد اكتمال التدريب/تقييم الأثر — دائرة التدريب
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> CloseNeed(int id)
        {
            var need = await _context.TrainingNeeds.FindAsync(id);
            if (need == null) return NotFound();
            if (need.ApprovalStatus != TrainingNeedApprovalStatus.TrainingCompleted &&
                need.ApprovalStatus != TrainingNeedApprovalStatus.ImpactAssessmentCompleted)
            {
                TempData["Error"] = "لا يمكن إغلاق الطلب في حالته الحالية";
                return RedirectToAction(nameof(Details), new { id });
            }

            var actor = await _userManager.GetUserAsync(User);
            need.ApprovalStatus = TrainingNeedApprovalStatus.Closed;
            need.Status = TrainingNeedStatus.Completed;
            AddHistory(need, need.ApprovalStatus, "إغلاق الطلب", null, actor);

            await _context.SaveChangesAsync();
            TempData["Success"] = "تم إغلاق الطلب";
            return RedirectToAction(nameof(Details), new { id });
        }

        // قائمة الاعتمادات المعلّقة
        public async Task<IActionResult> Approvals()
        {
            var actor = await _userManager.GetUserAsync(User);
            var myDept = actor?.Department;

            var query = _context.TrainingNeeds
                .Where(n => n.ApprovalStatus == TrainingNeedApprovalStatus.SubmittedToManager ||
                            n.ApprovalStatus == TrainingNeedApprovalStatus.ManagerApproved);

            if (!IsPrivileged)
                query = query.Where(n => n.Department == myDept);

            var needs = await query
                .OrderBy(n => n.ApprovalStatus)
                .ThenByDescending(n => n.SubmittedAt)
                .ToListAsync();

            ViewBag.IsPrivileged = IsPrivileged;
            return View(needs);
        }

        // ==================== لوحة مؤشرات TNA ====================

        public async Task<IActionResult> Dashboard(int? year, string? department, string? jobTitle,
            string? category, int? priority, int? status, int? gapType, decimal? roiReturn)
        {
            var actor = await _userManager.GetUserAsync(User);
            var myDept = actor?.Department;

            var query = _context.TrainingNeeds.AsQueryable();

            // نطاق الوصول: غير المخوّل يرى دائرته فقط
            if (!IsPrivileged)
                query = query.Where(n => n.Department == myDept);
            else if (!string.IsNullOrWhiteSpace(department))
                query = query.Where(n => n.Department == department);

            // الفلاتر
            if (year.HasValue)
                query = query.Where(n => n.CreatedAt.Year == year.Value);
            if (!string.IsNullOrWhiteSpace(jobTitle))
                query = query.Where(n => n.JobTitle == jobTitle);
            if (!string.IsNullOrWhiteSpace(category))
                query = query.Where(n => n.Category == category);
            if (priority.HasValue)
                query = query.Where(n => (int)n.Priority == priority.Value);
            if (status.HasValue)
                query = query.Where(n => (int)n.ApprovalStatus == status.Value);
            if (gapType.HasValue)
                query = query.Where(n => n.GapType != null && (int)n.GapType == gapType.Value);

            var needs = await query.ToListAsync();

            var totalRequired = needs.Sum(n => n.RequiredLevel);
            var totalCurrent = needs.Sum(n => Math.Min(n.CurrentLevel, n.RequiredLevel));

            // الميزانية المركزية للسنة المالية الحالية (للمؤشر % المستخدم)
            var budget = await GetOrmCurrentBudgetAsync();
            var estimatedTotal = needs.Sum(n => n.EstimatedCost > 0 ? n.EstimatedCost : n.PlannedCost);
            var budgetUsed = budget != null ? budget.CommittedBudget + budget.ActualSpending : 0m;
            var budgetAllocated = budget?.AllocatedBudget ?? 0m;

            var vm = new TnaDashboardViewModel
            {
                TotalNeeds = needs.Count,
                EmployeeCount = needs.Select(n => (n.EmployeeNumber ?? "") + "|" + n.EmployeeName).Distinct().Count(),
                HighPriorityCount = needs.Count(n => n.Priority == TrainingNeedPriority.High || n.Priority == TrainingNeedPriority.Critical),
                CriticalCount = needs.Count(n => n.Priority == TrainingNeedPriority.Critical),
                PendingApprovals = needs.Count(n => n.ApprovalStatus == TrainingNeedApprovalStatus.SubmittedToManager ||
                                                    n.ApprovalStatus == TrainingNeedApprovalStatus.ManagerApproved ||
                                                    n.ApprovalStatus == TrainingNeedApprovalStatus.BudgetReview),
                ApprovedCount = needs.Count(n => n.ApprovalStatus == TrainingNeedApprovalStatus.HRApproved),
                Readiness = totalRequired > 0 ? (int)Math.Round(100.0 * totalCurrent / totalRequired) : 0,
                EstimatedTotalCost = estimatedTotal,
                BudgetAllocated = budgetAllocated,
                BudgetUsed = budgetUsed,
                BudgetUtilization = budgetAllocated > 0 ? (int)Math.Round(100m * budgetUsed / budgetAllocated) : 0,
                ComplianceCount = needs.Count(n => n.IsCompliance ||
                                                   n.TrainingReason == TrainingReason.MandatoryRequirement ||
                                                   (n.GapType != null && n.GapType == SkillGapType.Compliance)),
                PerformanceLinkedCount = needs.Count(n => n.TrainingReason == TrainingReason.PerformanceWeakness ||
                                                          n.TrainingReason == TrainingReason.GoalFailure),
                GapByDepartment = needs
                    .GroupBy(n => n.Department)
                    .Select(g => new DepartmentGap
                    {
                        Department = g.Key,
                        AverageGap = Math.Round(g.Average(x => (double)Math.Max(0, x.RequiredLevel - x.CurrentLevel)), 1)
                    })
                    .OrderByDescending(d => d.AverageGap)
                    .ToList(),
                DepartmentDistribution = needs
                    .GroupBy(n => n.Department)
                    .OrderByDescending(g => g.Count())
                    .ToDictionary(g => g.Key, g => g.Count()),
                PriorityDistribution = new Dictionary<string, int>
                {
                    ["حرجة جداً"] = needs.Count(n => n.Priority == TrainingNeedPriority.Critical),
                    ["عالية"] = needs.Count(n => n.Priority == TrainingNeedPriority.High),
                    ["متوسطة"] = needs.Count(n => n.Priority == TrainingNeedPriority.Medium),
                    ["منخفضة"] = needs.Count(n => n.Priority == TrainingNeedPriority.Low),
                },
                CategoryDistribution = needs
                    .Where(n => !string.IsNullOrWhiteSpace(n.Category))
                    .GroupBy(n => n.Category!)
                    .OrderByDescending(g => g.Count())
                    .ToDictionary(g => g.Key, g => g.Count()),
                GapTypeDistribution = needs
                    .Where(n => n.GapType != null)
                    .GroupBy(n => n.GapType!.Value.DisplayName())
                    .ToDictionary(g => g.Key, g => g.Count()),
                ApprovalDistribution = needs
                    .GroupBy(n => TrainingNeed.GetApprovalStatusDisplayName(n.ApprovalStatus))
                    .ToDictionary(g => g.Key, g => g.Count()),
                TopSkills = needs
                    .GroupBy(n => n.SkillName)
                    .Select(g => new SkillCount { SkillName = g.Key, Count = g.Count() })
                    .OrderByDescending(s => s.Count)
                    .Take(10)
                    .ToList(),
                TopSkillGaps = needs
                    .GroupBy(n => n.SkillName)
                    .Select(g => new SkillGap
                    {
                        SkillName = g.Key,
                        AverageGap = Math.Round(g.Average(x => (double)Math.Max(0, x.RequiredLevel - x.CurrentLevel)), 1),
                        Count = g.Count()
                    })
                    .OrderByDescending(s => s.AverageGap)
                    .ThenByDescending(s => s.Count)
                    .Take(10)
                    .ToList(),
                RecentNeeds = needs.OrderByDescending(n => n.CreatedAt).Take(8).ToList(),
                IsPrivileged = IsPrivileged
            };

            // ===== محرك مؤشرات الأداء (KPI) =====
            string EmpKey(TrainingNeed n) => (n.EmployeeNumber ?? "") + "|" + n.EmployeeName;

            var completedStatuses = new[]
            {
                TrainingNeedApprovalStatus.TrainingCompleted,
                TrainingNeedApprovalStatus.ImpactAssessmentPending,
                TrainingNeedApprovalStatus.ImpactAssessmentCompleted,
                TrainingNeedApprovalStatus.Closed
            };

            // KPI 1 — نسبة التغطية: موظفون أُكمل تدريبهم ÷ إجمالي الموظفين المرصود لهم احتياج
            var trainedEmployees = needs.Where(n => completedStatuses.Contains(n.ApprovalStatus))
                .Select(EmpKey).Distinct().Count();
            var targetedEmployees = needs.Select(EmpKey).Distinct().Count();

            // KPI 3 — تحسن الأداء: من تقييمات الأثر المكتملة للاحتياجات المفلترة
            var needIds = needs.Select(n => n.Id).ToList();
            var assessments = await _context.TrainingImpactAssessments
                .Where(a => needIds.Contains(a.TrainingNeedId) && a.CompletedAt != null)
                .Select(a => new { a.TrainingNeedId, a.BeforeScore, a.AfterScore })
                .ToListAsync();

            var needToEmp = needs.ToDictionary(n => n.Id, EmpKey);
            var assessedEmp = assessments.Select(a => needToEmp[a.TrainingNeedId]).Distinct().Count();
            var improvedEmp = assessments.Where(a => a.AfterScore > a.BeforeScore)
                .Select(a => needToEmp[a.TrainingNeedId]).Distinct().Count();

            // KPI 4 — ROI اختياري: يُحسب فقط عند تمرير عائد مالي (لا يُخزَّن)
            var roiCost = budget != null && budget.ActualSpending > 0 ? budget.ActualSpending : estimatedTotal;

            vm.Kpis = new KpiEngine
            {
                TrainedEmployees = trainedEmployees,
                TargetedEmployees = targetedEmployees,
                HasBudget = budget != null && budgetAllocated > 0,
                BudgetPlanned = budgetAllocated,
                BudgetActual = budget != null ? budget.ActualSpending : 0m,
                AssessedEmployees = assessedEmp,
                ImprovedEmployees = improvedEmp,
                RoiAvailable = roiReturn.HasValue && roiReturn.Value > 0,
                RoiFinancialReturn = roiReturn ?? 0m,
                RoiTrainingCost = roiCost
            };
            ViewBag.RoiReturn = roiReturn;

            // خيارات الفلاتر (على كامل نطاق وصول المستخدم)
            var scope = _context.TrainingNeeds.AsQueryable();
            if (!IsPrivileged) scope = scope.Where(n => n.Department == myDept);

            ViewBag.Departments = await GetDepartmentsAsync();
            ViewBag.Years = await scope.Select(n => n.CreatedAt.Year).Distinct().OrderByDescending(y => y).ToListAsync();
            ViewBag.JobTitles = await scope.Where(n => n.JobTitle != null && n.JobTitle != "")
                .Select(n => n.JobTitle!).Distinct().OrderBy(j => j).ToListAsync();
            ViewBag.Categories = await scope.Where(n => n.Category != null && n.Category != "")
                .Select(n => n.Category!).Distinct().OrderBy(c => c).ToListAsync();

            ViewBag.CurrentDepartment = IsPrivileged ? department : myDept;
            ViewBag.FilterYear = year;
            ViewBag.FilterJobTitle = jobTitle;
            ViewBag.FilterCategory = category;
            ViewBag.FilterPriority = priority;
            ViewBag.FilterStatus = status;
            ViewBag.FilterGapType = gapType;
            return View(vm);
        }

        // ==================== تحليل الفجوات ====================

        public async Task<IActionResult> GapAnalysis(string? department)
        {
            var actor = await _userManager.GetUserAsync(User);
            var myDept = actor?.Department;

            var query = _context.TrainingNeeds.AsQueryable();
            if (!IsPrivileged)
                query = query.Where(n => n.Department == myDept);
            else if (!string.IsNullOrWhiteSpace(department))
                query = query.Where(n => n.Department == department);

            var needs = await query
                .OrderByDescending(n => n.Priority)
                .ThenBy(n => n.Department)
                .ToListAsync();

            var activePrograms = await _context.TrainingPrograms
                .Where(p => p.Status == ProgramStatus.Active)
                .Select(p => new { p.Title, p.Categories })
                .ToListAsync();

            var suggestions = new Dictionary<int, ProgramSuggestion>();
            foreach (var n in needs)
                suggestions[n.Id] = SuggestProgram(n, activePrograms.Select(p => (p.Title, p.Categories)).ToList());

            ViewBag.Suggestions = suggestions;
            ViewBag.IsPrivileged = IsPrivileged;
            ViewBag.Departments = await GetDepartmentsAsync();
            ViewBag.CurrentDepartment = IsPrivileged ? department : myDept;
            return View(needs);
        }

        // ==================== مصفوفة المهارات ====================

        public async Task<IActionResult> SkillsMatrix(string? department)
        {
            var actor = await _userManager.GetUserAsync(User);
            var myDept = actor?.Department;

            var query = _context.TrainingNeeds.AsQueryable();
            if (!IsPrivileged)
                query = query.Where(n => n.Department == myDept);
            else if (!string.IsNullOrWhiteSpace(department))
                query = query.Where(n => n.Department == department);

            var needs = await query.ToListAsync();

            var skills = needs.Select(n => n.SkillName).Distinct().OrderBy(s => s).ToList();

            var rows = needs
                .GroupBy(n => new { n.EmployeeName, n.Department })
                .Select(g =>
                {
                    var cells = new Dictionary<string, SkillMatrixCell>();
                    foreach (var n in g)
                    {
                        cells[n.SkillName] = new SkillMatrixCell
                        {
                            SkillName = n.SkillName,
                            RequiredLevel = n.RequiredLevel,
                            CurrentLevel = n.CurrentLevel
                        };
                    }
                    var req = g.Sum(x => x.RequiredLevel);
                    var cur = g.Sum(x => Math.Min(x.CurrentLevel, x.RequiredLevel));
                    return new SkillMatrixRow
                    {
                        EmployeeName = g.Key.EmployeeName,
                        Department = g.Key.Department,
                        Cells = cells,
                        AverageReadiness = req > 0 ? (int)Math.Round(100.0 * cur / req) : 0
                    };
                })
                .OrderBy(r => r.EmployeeName)
                .ToList();

            var vm = new SkillMatrixViewModel
            {
                Skills = skills,
                Rows = rows,
                IsPrivileged = IsPrivileged,
                CurrentDepartment = IsPrivileged ? department : myDept,
                Departments = await GetDepartmentsAsync()
            };
            return View(vm);
        }

        // ==================== الأولويات ====================

        public async Task<IActionResult> Priorities(string? department)
        {
            var actor = await _userManager.GetUserAsync(User);
            var myDept = actor?.Department;

            var query = _context.TrainingNeeds
                .Where(n => n.Priority == TrainingNeedPriority.High || n.Priority == TrainingNeedPriority.Critical);
            if (!IsPrivileged)
                query = query.Where(n => n.Department == myDept);
            else if (!string.IsNullOrWhiteSpace(department))
                query = query.Where(n => n.Department == department);

            var needs = await query
                .OrderByDescending(n => n.RequiredLevel - n.CurrentLevel)
                .ThenBy(n => n.Department)
                .ToListAsync();

            ViewBag.IsPrivileged = IsPrivileged;
            ViewBag.Departments = await GetDepartmentsAsync();
            ViewBag.CurrentDepartment = IsPrivileged ? department : myDept;
            return View(needs);
        }

        // البرامج التدريبية المرتبطة بالاحتياجات — عرض الاحتياجات المربوطة ببرامج/دفعات
        [HttpGet]
        public async Task<IActionResult> LinkedPrograms(string? department)
        {
            var actor = await _userManager.GetUserAsync(User);
            var myDept = actor?.Department;

            var query = _context.TrainingNeeds
                .Include(n => n.LinkedTrainingProgram)
                .Include(n => n.TrainingBatch)
                    .ThenInclude(b => b!.TrainingProgram)
                .Where(n => n.LinkedTrainingProgramId != null);
            if (!IsPrivileged)
                query = query.Where(n => n.Department == myDept);
            else if (!string.IsNullOrWhiteSpace(department))
                query = query.Where(n => n.Department == department);

            var needs = await query
                .OrderBy(n => n.LinkedTrainingProgram!.Title)
                .ThenBy(n => n.Department)
                .ToListAsync();

            ViewBag.IsPrivileged = IsPrivileged;
            ViewBag.Departments = await GetDepartmentsAsync();
            ViewBag.CurrentDepartment = IsPrivileged ? department : myDept;
            return View(needs);
        }

        // ==================== قالب Excel ====================

        private static readonly string[] TemplateHeaders =
        {
            "الرقم الوظيفي",
            "اسم الموظف *",
            "الدائرة *",
            "المهارة *",
            "التصنيف",
            "المستوى المطلوب (1-5) *",
            "المستوى الحالي (0-5) *",
            "ملاحظات"
        };

        public IActionResult DownloadTemplate()
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("الاحتياجات");
            ws.RightToLeft = true;

            for (int i = 0; i < TemplateHeaders.Length; i++)
            {
                var cell = ws.Cell(1, i + 1);
                cell.Value = TemplateHeaders[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1e3a5f");
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            ws.Cell(2, 1).Value = "EMP-1024";
            ws.Cell(2, 2).Value = "أحمد المطيري";
            ws.Cell(2, 3).Value = "تقنية المعلومات";
            ws.Cell(2, 4).Value = "إدارة قواعد البيانات";
            ws.Cell(2, 5).Value = "تقنية";
            ws.Cell(2, 6).Value = 4;
            ws.Cell(2, 7).Value = 2;
            ws.Cell(2, 8).Value = "بحاجة لتدريب متقدم";

            ws.Columns().AdjustToContents();
            for (int i = 1; i <= TemplateHeaders.Length; i++)
            {
                if (ws.Column(i).Width > 40) ws.Column(i).Width = 40;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"training-needs-template-{DateTime.Now:yyyyMMdd}.xlsx");
        }

        // ==================== استيراد Excel ====================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportPreview(IFormFile? file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "الرجاء اختيار ملف Excel صالح";
                return RedirectToAction(nameof(Index));
            }

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (ext != ".xlsx" && ext != ".xls")
            {
                TempData["Error"] = "صيغة الملف غير مدعومة. الرجاء رفع ملف بصيغة .xlsx";
                return RedirectToAction(nameof(Index));
            }

            var actor = await _userManager.GetUserAsync(User);
            var myDept = actor?.Department;

            List<TrainingNeedImportRow> rows;
            try
            {
                rows = ParseExcel(file, IsPrivileged ? null : myDept);
            }
            catch
            {
                TempData["Error"] = "تعذّر قراءة الملف. تأكد من استخدام القالب الصحيح";
                return RedirectToAction(nameof(Index));
            }

            if (rows.Count == 0)
            {
                TempData["Error"] = "الملف لا يحتوي على بيانات";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.FileName = Path.GetFileName(file.FileName);
            return View("ImportPreview", rows);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportConfirm(List<TrainingNeedImportRow> rows, string? fileName)
        {
            if (rows == null || rows.Count == 0)
            {
                TempData["Error"] = "لا توجد بيانات للاستيراد";
                return RedirectToAction(nameof(Index));
            }

            var actor = await _userManager.GetUserAsync(User);
            var myDept = actor?.Department;

            var validRows = new List<TrainingNeedImportRow>();
            foreach (var row in rows)
            {
                // المشرف: تُفرض دائرته على كل صف (لا يستورد ��دوائر أخرى)
                if (!IsPrivileged) row.Department = myDept;
                Validate(row);
                if (row.IsValid) validRows.Add(row);
            }

            if (validRows.Count == 0)
            {
                TempData["Error"] = "لم يتم استيراد أي س��ل. تحقق من صحة البيانات";
                return RedirectToAction(nameof(Index));
            }

            // دائرة الدفعة: للمشرف دائرته، وللمدير أول دائرة صالحة في الملف
            var batchDept = IsPrivileged
                ? (validRows.FirstOrDefault()?.Department ?? "غير محدد")
                : (myDept ?? "غير محدد");

            var batch = new TrainingNeedBatch
            {
                FileName = fileName,
                Department = batchDept,
                RecordCount = validRows.Count,
                CreatedByUserId = actor?.Id,
                CreatedAt = DateTime.Now
            };
            _context.TrainingNeedBatches.Add(batch);

            foreach (var row in validRows)
            {
                _context.TrainingNeeds.Add(new TrainingNeed
                {
                    EmployeeName = row.EmployeeName!.Trim(),
                    EmployeeNumber = row.EmployeeNumber?.Trim(),
                    Department = row.Department!.Trim(),
                    SkillName = row.SkillName!.Trim(),
                    Category = row.Category?.Trim(),
                    RequiredLevel = row.RequiredLevel,
                    CurrentLevel = row.CurrentLevel,
                    Priority = TrainingNeed.ComputePriority(row.RequiredLevel, row.CurrentLevel),
                    Status = TrainingNeedStatus.New,
                    Notes = row.Notes?.Trim(),
                    CreatedByUserId = actor?.Id,
                    CreatedAt = DateTime.Now,
                    Batch = batch
                });
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = $"تم استيراد {validRows.Count} احتياج تدريبي بنجاح";
            return RedirectToAction(nameof(Index));
        }

        // ==================== التقارير ====================

        public async Task<IActionResult> Reports(string? department)
        {
            var actor = await _userManager.GetUserAsync(User);
            var myDept = actor?.Department;

            var query = _context.TrainingNeeds.AsQueryable();
            if (!IsPrivileged)
                query = query.Where(n => n.Department == myDept);
            else if (!string.IsNullOrWhiteSpace(department))
                query = query.Where(n => n.Department == department);

            var needs = await query.ToListAsync();

            // تقرير حسب الدائرة
            var byDept = needs
                .GroupBy(n => n.Department)
                .Select(g => new DepartmentReportRow
                {
                    Department = g.Key,
                    NeedsCount = g.Count(),
                    EmployeeCount = g.Select(x => (x.EmployeeNumber ?? "") + "|" + x.EmployeeName).Distinct().Count(),
                    HighPriority = g.Count(x => x.Priority == TrainingNeedPriority.High),
                    AverageGap = Math.Round(g.Average(x => (double)Math.Max(0, x.RequiredLevel - x.CurrentLevel)), 1),
                    Readiness = g.Sum(x => x.RequiredLevel) > 0
                        ? (int)Math.Round(100.0 * g.Sum(x => Math.Min(x.CurrentLevel, x.RequiredLevel)) / g.Sum(x => x.RequiredLevel))
                        : 0
                })
                .OrderByDescending(r => r.AverageGap)
                .ToList();

            // تقرير حسب التصنيف
            var byCategory = needs
                .Where(n => !string.IsNullOrWhiteSpace(n.Category))
                .GroupBy(n => n.Category!)
                .Select(g => new CategoryReportRow
                {
                    Category = g.Key,
                    NeedsCount = g.Count(),
                    HighPriority = g.Count(x => x.Priority == TrainingNeedPriority.High),
                    AverageGap = Math.Round(g.Average(x => (double)Math.Max(0, x.RequiredLevel - x.CurrentLevel)), 1)
                })
                .OrderByDescending(r => r.NeedsCount)
                .ToList();

            // أكثر المهارات طلباً
            var topSkills = needs
                .GroupBy(n => n.SkillName)
                .Select(g => new SkillReportRow
                {
                    SkillName = g.Key,
                    NeedsCount = g.Count(),
                    AverageGap = Math.Round(g.Average(x => (double)Math.Max(0, x.RequiredLevel - x.CurrentLevel)), 1)
                })
                .OrderByDescending(r => r.NeedsCount)
                .Take(10)
                .ToList();

            // توزيع حسب الأولوية
            var byPriority = needs
                .GroupBy(n => n.Priority)
                .Select(g => new LabelCountRow
                {
                    Label = TrainingNeed.GetPriorityDisplayName(g.Key),
                    Count = g.Count(),
                    Color = TrainingNeed.GetPriorityBadge(g.Key)
                })
                .OrderByDescending(r => r.Count)
                .ToList();

            // توزيع حسب حالة الاعتماد
            var byStatus = needs
                .GroupBy(n => n.ApprovalStatus)
                .Select(g => new LabelCountRow
                {
                    Label = TrainingNeed.GetApprovalStatusDisplayName(g.Key),
                    Count = g.Count(),
                    Color = TrainingNeed.GetApprovalStatusBadge(g.Key)
                })
                .OrderByDescending(r => r.Count)
                .ToList();

            // التكلفة التق��يرية حسب الدائرة
            var costByDept = needs
                .GroupBy(n => n.Department)
                .Select(g => new CostReportRow
                {
                    Department = g.Key,
                    NeedsCount = g.Count(),
                    EstimatedCost = g.Sum(x => x.EstimatedCost)
                })
                .OrderByDescending(r => r.EstimatedCost)
                .ToList();

            // أعلى الفجوات على مستوى الموظف
            var topGaps = needs
                .Select(n => new EmployeeGapRow
                {
                    EmployeeName = n.EmployeeName,
                    Department = n.Department,
                    SkillName = n.SkillName,
                    Gap = Math.Max(0, n.RequiredLevel - n.CurrentLevel),
                    PriorityScore = n.PriorityScore
                })
                .OrderByDescending(r => r.Gap)
                .ThenByDescending(r => r.PriorityScore)
                .Take(10)
                .ToList();

            // الموازنة والأثر مؤجّلة في المرحلة الحالية — لا تُحسب هنا

            var vm = new TnaReportsViewModel
            {
                ByDepartment = byDept,
                ByCategory = byCategory,
                TopSkills = topSkills,
                ByPriority = byPriority,
                ByStatus = byStatus,
                CostByDepartment = costByDept,
                TopEmployeeGaps = topGaps,
                ComplianceNeeds = needs.Count(n => n.IsCompliance),
                TotalEstimatedCost = needs.Sum(n => n.EstimatedCost),
                TotalNeeds = needs.Count,
                IsPrivileged = IsPrivileged,
                CurrentDepartment = IsPrivileged ? department : myDept
            };

            ViewBag.Departments = await GetDepartmentsAsync();
            return View(vm);
        }

        // تصدير الاحتياجات إلى Excel
        public async Task<IActionResult> ExportNeeds(string? department)
        {
            var actor = await _userManager.GetUserAsync(User);
            var myDept = actor?.Department;

            var query = _context.TrainingNeeds.AsQueryable();
            if (!IsPrivileged)
                query = query.Where(n => n.Department == myDept);
            else if (!string.IsNullOrWhiteSpace(department))
                query = query.Where(n => n.Department == department);

            var needs = await query
                .OrderBy(n => n.Department)
                .ThenByDescending(n => n.Priority)
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("الاحتياجات التدريبية");
            ws.RightToLeft = true;

            string[] headers =
            {
                "الرقم الوظيفي", "اسم الموظف", "المسمى الوظيفي", "الدرجة", "الدائرة",
                "المهارة", "التصنيف", "المستوى المطلوب", "المستوى الحالي", "الفجوة",
                "الأولوية", "حالة الاعتماد", "مبرر الاحتياج"
            };
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cell(1, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1e3a5f");
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            int r = 2;
            foreach (var n in needs)
            {
                ws.Cell(r, 1).Value = n.EmployeeNumber ?? "";
                ws.Cell(r, 2).Value = n.EmployeeName;
                ws.Cell(r, 3).Value = n.JobTitle ?? "";
                ws.Cell(r, 4).Value = n.Grade ?? "";
                ws.Cell(r, 5).Value = n.Department;
                ws.Cell(r, 6).Value = n.SkillName;
                ws.Cell(r, 7).Value = n.Category ?? "";
                ws.Cell(r, 8).Value = n.RequiredLevel;
                ws.Cell(r, 9).Value = n.CurrentLevel;
                ws.Cell(r, 10).Value = n.Gap;
                ws.Cell(r, 11).Value = TrainingNeed.GetPriorityDisplayName(n.Priority);
                ws.Cell(r, 12).Value = TrainingNeed.GetApprovalStatusDisplayName(n.ApprovalStatus);
                ws.Cell(r, 13).Value = n.Justification ?? "";
                r++;
            }

            ws.Columns().AdjustToContents();
            for (int i = 1; i <= headers.Length; i++)
                if (ws.Column(i).Width > 40) ws.Column(i).Width = 40;

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"training-needs-{DateTime.Now:yyyyMMdd}.xlsx");
        }

        // ==================== مساعدات ====================

        // تحقق صلاحية الوصول لسجل معيّن (منع IDOR): المشرف لدائرته فقط
        private async Task<bool> CanAccessAsync(TrainingNeed need)
        {
            if (IsPrivileged) return true;
            var actor = await _userManager.GetUserAsync(User);
            return actor?.Department != null && need.Department == actor.Department;
        }

        private async Task<List<string>> GetDepartmentsAsync()
        {
            return await _context.Users
                .Where(u => u.Department != null && u.Department != "")
                .Select(u => u.Department!)
                .Distinct()
                .OrderBy(d => d)
                .ToListAsync();
        }

        // معرّفات مستخدمي دائرة التدريب (Admin + SuperAdmin)
        private async Task<List<string>> GetHRUserIdsAsync()
        {
            var admins = await _userManager.GetUsersInRoleAsync(SystemRoles.Admin);
            var supers = await _userManager.GetUsersInRoleAsync(SystemRoles.SuperAdmin);
            return admins.Concat(supers).Select(u => u.Id).Distinct().ToList();
        }

        // معرّفات المدراء المسؤولين عن دائرة معيّنة (مشرفو الدائرة + دائرة التدريب)
        private async Task<List<string>> GetDepartmentManagerIdsAsync(string department)
        {
            var supervisors = await _userManager.GetUsersInRoleAsync(SystemRoles.Supervisor);
            var deptSupervisors = supervisors
                .Where(u => u.Department == department)
                .Select(u => u.Id);
            var hr = await GetHRUserIdsAsync();
            return deptSupervisors.Concat(hr).Distinct().ToList();
        }

        // ا��سنة المالية الحالية (تق��يم��ة) بصيغة 2025/2026
        private static string CurrentFinancialYear()
        {
            var y = DateTime.Now.Year;
            // السنة المالية تبدأ يناير — ��مكن تعديلها لاحقاً لتبدأ من ��هر آخر
            return $"{y}/{y + 1}";
        }

        // الموازنة المركزية للسن�� المالية الحالية (قد تكون null إن لم تُعرّف)
        private async Task<TrainingBudget?> GetOrmCurrentBudgetAsync()
        {
            var fy = CurrentFinancialYear();
            return await _context.TrainingBudgets
                .FirstOrDefaultAsync(b => b.FinancialYear == fy);
        }

        // تكلفة الاحتياج المعتمدة لأغراض الميزانية (التقديرية أولاً ثم المخططة)
        private static decimal GetNeedBudgetCost(TrainingNeed need)
            => need.EstimatedCost > 0 ? need.EstimatedCost : need.PlannedCost;

        // يضمن اتساق سلسلة: البرنامج ← الدفعة. إن اختيرت دفعة، يُشتق برنامجها تلقائياً.
        private async Task AlignBatchToProgramAsync(TrainingNeed need)
        {
            if (need.TrainingBatchId == null) return;

            var batch = await _context.Batches
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == need.TrainingBatchId);

            if (batch == null)
            {
                need.TrainingBatchId = null;
                return;
            }

            // الدفعة تابعة لبرنامج — اجعل برنامج الاحتياج هو برنامج الدفعة
            need.LinkedTrainingProgramId = batch.TrainingProgramId;
        }

        // بيانات الموظف للعرض التلقائي ف�� نموذج الاحتياج (تُقرأ عبر FK — لا تُكرَّر)
        [HttpGet]
        public async Task<IActionResult> EmployeeInfo(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return Json(new { ok = false });

            var employee = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id && u.IsActive);
            if (employee == null)
                return Json(new { ok = false });

            // المشر�� يرى موظفي دائرته فقط
            if (!IsPrivileged)
            {
                var actor = await _userManager.GetUserAsync(User);
                if (employee.Department != actor?.Department)
                    return Json(new { ok = false });
            }

            string? managerName = null;
            if (!string.IsNullOrWhiteSpace(employee.ManagerUserId))
            {
                managerName = await _context.Users
                    .AsNoTracking()
                    .Where(u => u.Id == employee.ManagerUserId)
                    .Select(u => u.FullName)
                    .FirstOrDefaultAsync();
            }

            return Json(new
            {
                ok = true,
                employeeNumber = employee.EmployeeNumber ?? employee.UserName,
                fullName = employee.FullName,
                department = employee.Department,
                jobTitle = employee.JobTitle,
                grade = employee.Grade,
                managerName
            });
        }

        // إن اختيرت مهارة من المكتبة، يُشتقّ تصنيف الاحتياج ونوع الفجوة من تصنيف المهارة
        private async Task AlignSkillCategoryAsync(TrainingNeed need)
        {
            // اشتقاق التصنيف من المهارة عند عدم تحديده
            if (need.SkillCategoryId == null && need.SkillId != null)
            {
                need.SkillCategoryId = await _context.Skills
                    .AsNoTracking()
                    .Where(s => s.Id == need.SkillId)
                    .Select(s => s.SkillCategoryId)
                    .FirstOrDefaultAsync();
            }

            if (need.SkillCategoryId == null) return;

            var category = await _context.SkillCategories
                .AsNoTracking()
                .Where(c => c.Id == need.SkillCategoryId)
                .Select(c => new { c.Name, c.GapType })
                .FirstOrDefaultAsync();

            if (category != null)
            {
                if (string.IsNullOrWhiteSpace(need.Category))
                    need.Category = category.Name;
                // إن لم يحدد المستخدم نوع الفجوة، يُشتقّ من نوع الفئة الكبرى للتصنيف
                if (need.GapType == null && category.GapType != null)
                    need.GapType = category.GapType;
            }
        }

        private async Task PopulateFormOptionsAsync(TrainingNeedFormViewModel vm)
        {
            var actor = await _userManager.GetUserAsync(User);
            var myDept = actor?.Department;

            var usersQuery = _context.Users.Where(u => u.IsActive);
            if (!IsPrivileged)
            {
                usersQuery = usersQuery.Where(u => u.Department == myDept);
            }

            var employees = await usersQuery
                .OrderBy(u => u.FullName)
                .Select(u => new { u.Id, u.FullName, u.Department })
                .ToListAsync();

            vm.Employees = employees.Select(e => new SelectListItem
            {
                Value = e.Id,
                Text = string.IsNullOrEmpty(e.Department)
                    ? e.FullName
                    : $"{e.FullName} — {e.Department}"
            }).ToList();

            var skills = await _context.Skills
                .Where(s => s.IsActive)
                .Include(s => s.SkillCategory)
                .OrderBy(s => s.Name)
                .ToListAsync();

            vm.Skills = skills.Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = s.SkillCategory != null ? $"{s.Name} ({s.SkillCategory.Name})" : s.Name
            }).ToList();

            vm.Categories = await _context.SkillCategories
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem { Value = c.Name, Text = c.Name })
                .ToListAsync();

            // تصنيفات المهارة مفهرسة بالمعرّف (للربط عبر FK)
            vm.SkillCategoryOptions = await _context.SkillCategories
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .ToListAsync();

            vm.Programs = await _context.TrainingPrograms
                .Where(p => p.Status == ProgramStatus.Active)
                .OrderBy(p => p.Title)
                .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Title })
                .ToListAsync();

            // الدفعات التدريبية المتاحة (غير الملغاة) مع اسم البرنامج التابعة له
            vm.Batches = await _context.Batches
                .Where(b => b.Status != BatchStatus.Cancelled)
                .Include(b => b.TrainingProgram)
                .OrderByDescending(b => b.StartDate)
                .Select(b => new SelectListItem
                {
                    Value = b.Id.ToString(),
                    Text = (b.TrainingProgram != null ? b.TrainingProgram.Title + " — " : "") +
                           b.Name + " (" + b.StartDate.ToString("yyyy/MM/dd") + ")"
                })
                .ToListAsync();

            vm.IsPrivileged = IsPrivileged;
            vm.LockedDepartment = IsPrivileged ? null : myDept;
        }

        // تسجيل مرحلة في سجل الاعتماد (لا يستدعي SaveChanges — يُترك للمستد��ي)
        private void AddHistory(TrainingNeed need, TrainingNeedApprovalStatus status,
            string action, string? comment, ApplicationUser? actor)
        {
            _context.TrainingNeedStatusHistories.Add(new TrainingNeedStatusHistory
            {
                TrainingNeedId = need.Id,
                Status = status,
                Action = action,
                Comment = comment,
                ActionByUserId = actor?.Id,
                ActionByName = actor?.FullName,
                ActionAt = DateTime.Now
            });
        }

        private List<TrainingNeedImportRow> ParseExcel(IFormFile file, string? forcedDepartment)
        {
            var rows = new List<TrainingNeedImportRow>();

            using var stream = new MemoryStream();
            file.CopyTo(stream);
            stream.Position = 0;

            using var workbook = new XLWorkbook(stream);
            var ws = workbook.Worksheet(1);
            var range = ws.RangeUsed();
            if (range == null) return rows;

            var lastRow = range.RowCount();
            for (int r = 2; r <= lastRow; r++)
            {
                string Cell(int c) => ws.Cell(r, c).GetString().Trim();
                int CellInt(int c)
                {
                    var raw = ws.Cell(r, c).GetString().Trim();
                    return int.TryParse(raw, out var v) ? v : -1;
                }

                var row = new TrainingNeedImportRow
                {
                    RowNumber = r,
                    EmployeeNumber = Cell(1),
                    EmployeeName = Cell(2),
                    Department = forcedDepartment ?? Cell(3),
                    SkillName = Cell(4),
                    Category = Cell(5),
                    RequiredLevel = CellInt(6),
                    CurrentLevel = CellInt(7),
                    Notes = Cell(8)
                };

                // تجاهل الصفوف الفارغة تماماً
                if (string.IsNullOrWhiteSpace(row.EmployeeName) &&
                    string.IsNullOrWhiteSpace(row.SkillName))
                {
                    continue;
                }

                Validate(row);
                rows.Add(row);
            }

            return rows;
        }

        // مطابقة الاحتياج بأنسب برنامج: أولاً بالتصنيف ثم بالكلمات المفتاحية في العنوان
        private static ProgramSuggestion SuggestProgram(TrainingNeed need, List<(string Title, string? Categories)> programs)
        {
            if (!string.IsNullOrWhiteSpace(need.Category))
            {
                var byCategory = programs.FirstOrDefault(p =>
                    !string.IsNullOrWhiteSpace(p.Categories) &&
                    p.Categories!.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        .Any(c => string.Equals(c, need.Category, StringComparison.OrdinalIgnoreCase)));
                if (byCategory.Title != null)
                    return new ProgramSuggestion { Title = byCategory.Title, Matched = true };
            }

            var words = (need.SkillName ?? "")
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Where(w => w.Length > 2)
                .ToList();
            var byKeyword = programs.FirstOrDefault(p =>
                words.Any(w => p.Title.Contains(w, StringComparison.OrdinalIgnoreCase)));
            if (byKeyword.Title != null)
                return new ProgramSuggestion { Title = byKeyword.Title, Matched = true };

            return new ProgramSuggestion { Title = "لا يوجد برنامج مطابق — يُنصح بإضافة برنامج", Matched = false };
        }

        private void Validate(TrainingNeedImportRow row)
        {
            row.Errors.Clear();

            if (string.IsNullOrWhiteSpace(row.EmployeeName))
                row.Errors.Add("اسم الموظف مطلوب");
            if (string.IsNullOrWhiteSpace(row.Department))
                row.Errors.Add("الدائرة مطلوبة");
            if (string.IsNullOrWhiteSpace(row.SkillName))
                row.Errors.Add("المهارة مطلوبة");
            if (row.RequiredLevel < 1 || row.RequiredLevel > 5)
                row.Errors.Add("المستوى المطلوب يجب أن يكون بين 1 و 5");
            if (row.CurrentLevel < 0 || row.CurrentLevel > 5)
                row.Errors.Add("المستوى الحالي يجب أن يكون بين 0 و 5");

            row.IsValid = row.Errors.Count == 0;
        }
    }

    // ==================== ViewModels ====================

    public class TrainingNeedsIndexViewModel
    {
        public List<TrainingNeed> Needs { get; set; } = new();
        public int TotalNeeds { get; set; }
        public int EmployeeCount { get; set; }
        public int HighPriorityCount { get; set; }
        public int InProgressCount { get; set; }
        public int Readiness { get; set; }
        public bool IsPrivileged { get; set; }
        public string? CurrentDepartment { get; set; }
        public string? Search { get; set; }
        public List<string> Departments { get; set; } = new();
        public Dictionary<int, ProgramSuggestion> Suggestions { get; set; } = new();
        public List<DepartmentGap> GapByDepartment { get; set; } = new();
    }

    public class ProgramSuggestion
    {
        public string Title { get; set; } = string.Empty;
        public bool Matched { get; set; }
    }

    public class DepartmentGap
    {
        public string Department { get; set; } = string.Empty;
        public double AverageGap { get; set; }
    }

    public class TrainingNeedFormViewModel
    {
        public int Id { get; set; }

        public string? EmployeeUserId { get; set; }
        public string? EmployeeName { get; set; }
        public string? EmployeeNumber { get; set; }
        public string? Department { get; set; }
        public string? SkillName { get; set; }
        public string? Category { get; set; }
        public int RequiredLevel { get; set; } = 3;
        public int CurrentLevel { get; set; } = 1;
        public TrainingNeedStatus Status { get; set; } = TrainingNeedStatus.New;
        public string? Notes { get; set; }

        // حقول TNA الإضافية
        public int? SkillId { get; set; }
        public string? JobTitle { get; set; }
        public string? Grade { get; set; }

        [Required(ErrorMessage = "يجب اختيار مبرر طلب التدريب")]
        public TrainingReason? TrainingReason { get; set; }
        public string? FailedGoalNumber { get; set; }
        public string? PerformanceGapDescription { get; set; }

        public string? Justification { get; set; }
        public string? SuggestedTimeframe { get; set; }
        public bool SubmitForApproval { get; set; }

        // حقول المرحلة 2: التكلفة والأثر والميزانية
        public int? LinkedTrainingProgramId { get; set; }
        public decimal EstimatedCost { get; set; }
        public int ParticipantsCount { get; set; } = 1;
        public int ImpactScore { get; set; } = 2;
        public int RiskScore { get; set; } = 2;
        public bool IsCompliance { get; set; }

        // ربط البرنامج/الدفعة والتكاليف والتواريخ الفعلية
        public int? TrainingBatchId { get; set; }
        public decimal PlannedCost { get; set; }
        public decimal ActualCost { get; set; }
        public DateTime? PlannedDate { get; set; }
        public DateTime? CompletionDate { get; set; }

        // حقول وصف الاحتياج (البند 4)
        public int? SkillCategoryId { get; set; }
        public string? NeedDescription { get; set; }
        public SkillGapType? GapType { get; set; }
        public string? ProposedTrainingProgram { get; set; }
        public string? ProposedProgramDescription { get; set; }
        public string? PreferredTrainingProvider { get; set; }
        public TrainingMode? TrainingMode { get; set; }
        public string? ProposedDuration { get; set; }

        // خيارات العرض
        public List<SelectListItem> Employees { get; set; } = new();
        public List<SelectListItem> Skills { get; set; } = new();
        public List<SelectListItem> Categories { get; set; } = new();
        public List<SelectListItem> SkillCategoryOptions { get; set; } = new();
        public List<SelectListItem> Programs { get; set; } = new();
        public List<SelectListItem> Batches { get; set; } = new();
        public bool IsPrivileged { get; set; }
        public string? LockedDepartment { get; set; }
    }

    // عنصر مصفوفة المهارات (موظف × م��ارة)
    public class SkillMatrixCell
    {
        public string SkillName { get; set; } = string.Empty;
        public int RequiredLevel { get; set; }
        public int CurrentLevel { get; set; }
        public int Gap => Math.Max(0, RequiredLevel - CurrentLevel);
    }

    public class SkillMatrixRow
    {
        public string EmployeeName { get; set; } = string.Empty;
        public string? Department { get; set; }
        public Dictionary<string, SkillMatrixCell> Cells { get; set; } = new();
        public int AverageReadiness { get; set; }
    }

    public class SkillMatrixViewModel
    {
        public List<string> Skills { get; set; } = new();
        public List<SkillMatrixRow> Rows { get; set; } = new();
        public bool IsPrivileged { get; set; }
        public string? CurrentDepartment { get; set; }
        public List<string> Departments { get; set; } = new();
    }

    // ==================== ViewModels التقارير ====================

    public class DepartmentReportRow
    {
        public string Department { get; set; } = string.Empty;
        public int NeedsCount { get; set; }
        public int EmployeeCount { get; set; }
        public int HighPriority { get; set; }
        public double AverageGap { get; set; }
        public int Readiness { get; set; }
    }

    public class CategoryReportRow
    {
        public string Category { get; set; } = string.Empty;
        public int NeedsCount { get; set; }
        public int HighPriority { get; set; }
        public double AverageGap { get; set; }
    }

    public class SkillReportRow
    {
        public string SkillName { get; set; } = string.Empty;
        public int NeedsCount { get; set; }
        public double AverageGap { get; set; }
    }

    public class LabelCountRow
    {
        public string Label { get; set; } = string.Empty;
        public int Count { get; set; }
        public string Color { get; set; } = "secondary";
    }

    public class CostReportRow
    {
        public string Department { get; set; } = string.Empty;
        public int NeedsCount { get; set; }
        public decimal EstimatedCost { get; set; }
    }

    public class BudgetComplianceRow
    {
        public string FinancialYear { get; set; } = string.Empty;
        public decimal Allocated { get; set; }
        public decimal Committed { get; set; }
        public decimal Actual { get; set; }
        public decimal Remaining => Allocated - Committed - Actual;
        public int Utilization => Allocated > 0
            ? (int)Math.Round(100m * (Committed + Actual) / Allocated) : 0;
    }

    public class ImpactResultRow
    {
        public string Type { get; set; } = string.Empty;
        public int Completed { get; set; }
        public int Pending { get; set; }
        public double AverageScore { get; set; }
        public double AverageImprovement { get; set; }
    }

    public class EmployeeGapRow
    {
        public string EmployeeName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string SkillName { get; set; } = string.Empty;
        public int Gap { get; set; }
        public int PriorityScore { get; set; }
    }

    public class TnaReportsViewModel
    {
        public List<DepartmentReportRow> ByDepartment { get; set; } = new();
        public List<CategoryReportRow> ByCategory { get; set; } = new();
        public List<SkillReportRow> TopSkills { get; set; } = new();
        public List<LabelCountRow> ByPriority { get; set; } = new();
        public List<LabelCountRow> ByStatus { get; set; } = new();
        public List<CostReportRow> CostByDepartment { get; set; } = new();
        public List<BudgetComplianceRow> BudgetCompliance { get; set; } = new();
        public List<ImpactResultRow> ImpactResults { get; set; } = new();
        public List<EmployeeGapRow> TopEmployeeGaps { get; set; } = new();
        public int ComplianceNeeds { get; set; }
        public decimal TotalEstimatedCost { get; set; }
        public int TotalNeeds { get; set; }
        public bool IsPrivileged { get; set; }
        public string? CurrentDepartment { get; set; }
    }

    // لوحة مؤشرات TNA
    public class TnaDashboardViewModel
    {
        // بطاقات المؤشرات
        public int TotalNeeds { get; set; }
        public int EmployeeCount { get; set; }
        public int HighPriorityCount { get; set; }
        public int PendingApprovals { get; set; }
        public int ApprovedCount { get; set; }
        public int CriticalCount { get; set; }
        public int Readiness { get; set; }
        public decimal EstimatedTotalCost { get; set; }
        public decimal BudgetAllocated { get; set; }
        public decimal BudgetUsed { get; set; }
        public int BudgetUtilization { get; set; }
        public int ComplianceCount { get; set; }
        public int PerformanceLinkedCount { get; set; }

        // توزيعات (للرسوم البيانية)
        public List<DepartmentGap> GapByDepartment { get; set; } = new();
        public Dictionary<string, int> DepartmentDistribution { get; set; } = new();
        public Dictionary<string, int> PriorityDistribution { get; set; } = new();
        public Dictionary<string, int> CategoryDistribution { get; set; } = new();
        public Dictionary<string, int> GapTypeDistribution { get; set; } = new();
        public Dictionary<string, int> ApprovalDistribution { get; set; } = new();
        public List<SkillCount> TopSkills { get; set; } = new();
        public List<SkillGap> TopSkillGaps { get; set; } = new();
        public List<TrainingNeed> RecentNeeds { get; set; } = new();
        public bool IsPrivileged { get; set; }

        // محرك مؤشرات الأداء (KPI)
        public KpiEngine Kpis { get; set; } = new();
    }

    // مؤشرات الأداء الأربعة المحسوبة للوحة المعلومات
    public class KpiEngine
    {
        // KPI 1 — نسبة تغطية الاحتياجات التدريبية
        public int TrainedEmployees { get; set; }
        public int TargetedEmployees { get; set; }
        public int CoveragePercent => TargetedEmployees > 0
            ? (int)Math.Round(100.0 * TrainedEmployees / TargetedEmployees) : 0;

        // KPI 2 — الالتزام بالميزانية
        public decimal BudgetPlanned { get; set; }
        public decimal BudgetActual { get; set; }
        public decimal BudgetVariance => BudgetPlanned - BudgetActual;
        public int BudgetUtilizationPercent => BudgetPlanned > 0
            ? (int)Math.Round(100m * BudgetActual / BudgetPlanned) : 0;
        public bool HasBudget { get; set; }

        // KPI 3 — معدل تحسن الأداء بعد التدريب
        public int ImprovedEmployees { get; set; }
        public int AssessedEmployees { get; set; }
        public int PerformanceImprovementPercent => AssessedEmployees > 0
            ? (int)Math.Round(100.0 * ImprovedEmployees / AssessedEmployees) : 0;

        // KPI 4 — العائد على الاستثمار (اختياري)
        public bool RoiAvailable { get; set; }
        public decimal RoiFinancialReturn { get; set; }
        public decimal RoiTrainingCost { get; set; }
        public int RoiPercent => RoiAvailable && RoiTrainingCost > 0
            ? (int)Math.Round(100m * (RoiFinancialReturn - RoiTrainingCost) / RoiTrainingCost) : 0;
    }

    public class SkillCount
    {
        public string SkillName { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class SkillGap
    {
        public string SkillName { get; set; } = string.Empty;
        public double AverageGap { get; set; }
        public int Count { get; set; }
    }
}
