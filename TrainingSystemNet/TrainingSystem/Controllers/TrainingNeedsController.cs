using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.Data;
using TrainingSystem.Models;

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
                HighPriorityCount = needs.Count(n => n.Priority == TrainingNeedPriority.High),
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
                    model.EmployeeNumber = employee.UserName;
                    model.Department = employee.Department;
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

            // المشرف لا يضيف احتياجاً لموظف خارج دائرته
            if (!IsPrivileged && employee != null && employee.Department != myDept)
                ModelState.AddModelError(string.Empty, "لا يمكنك إضافة احتياج لموظف خارج دائرتك");

            if (!ModelState.IsValid)
            {
                await PopulateFormOptionsAsync(model);
                return View(model);
            }

            var need = new TrainingNeed
            {
                EmployeeName = model.EmployeeName!.Trim(),
                EmployeeNumber = model.EmployeeNumber?.Trim(),
                Department = model.Department!.Trim(),
                SkillName = model.SkillName!.Trim(),
                Category = model.Category?.Trim(),
                RequiredLevel = model.RequiredLevel,
                CurrentLevel = model.CurrentLevel,
                Priority = TrainingNeed.ComputePriority(model.RequiredLevel, model.CurrentLevel),
                Status = TrainingNeedStatus.New,
                Notes = model.Notes?.Trim(),
                CreatedByUserId = actor?.Id,
                CreatedAt = DateTime.Now
            };

            _context.TrainingNeeds.Add(need);
            await _context.SaveChangesAsync();

            TempData["Success"] = "تم إضافة الاحتياج التدريبي بنجاح";
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
                EmployeeName = need.EmployeeName,
                EmployeeNumber = need.EmployeeNumber,
                Department = need.Department,
                SkillName = need.SkillName,
                Category = need.Category,
                RequiredLevel = need.RequiredLevel,
                CurrentLevel = need.CurrentLevel,
                Status = need.Status,
                Notes = need.Notes
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

            // الدائرة لا تتغيّر عبر التعديل (تبقى كما هي لضمان بقاء السجل ضمن نطاق الدائرة)
            need.EmployeeName = model.EmployeeName!.Trim();
            need.EmployeeNumber = model.EmployeeNumber?.Trim();
            need.SkillName = model.SkillName!.Trim();
            need.Category = model.Category?.Trim();
            need.RequiredLevel = model.RequiredLevel;
            need.CurrentLevel = model.CurrentLevel;
            need.Priority = TrainingNeed.ComputePriority(model.RequiredLevel, model.CurrentLevel);
            need.Status = model.Status;
            need.Notes = model.Notes?.Trim();

            await _context.SaveChangesAsync();
            TempData["Success"] = "تم تحديث الاحتياج التدريبي بنجاح";
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
                // المشرف: تُفرض دائرته على كل صف (لا يستورد لدوائر أخرى)
                if (!IsPrivileged) row.Department = myDept;
                Validate(row);
                if (row.IsValid) validRows.Add(row);
            }

            if (validRows.Count == 0)
            {
                TempData["Error"] = "لم يتم استيراد أي سجل. تحقق من صحة البيانات";
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

            vm.IsPrivileged = IsPrivileged;
            vm.LockedDepartment = IsPrivileged ? null : myDept;
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

        // خيارات العرض
        public List<SelectListItem> Employees { get; set; } = new();
        public bool IsPrivileged { get; set; }
        public string? LockedDepartment { get; set; }
    }
}
