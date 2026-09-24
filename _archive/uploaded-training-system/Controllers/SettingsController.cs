using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using TrainingSystem.Data;
using TrainingSystem.Models;

namespace TrainingSystem.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin,Supervisor")]
    public class SettingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public SettingsController(
            ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // صفحة الإعدادات الرئيسية
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> Index()
        {
            var settings = await _context.SystemSettings.FirstOrDefaultAsync();
            return View(settings ?? new SystemSettings());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> UpdateSettings(SystemSettings model)
        {
            var settings = await _context.SystemSettings.FirstOrDefaultAsync();

            if (settings == null)
            {
                _context.SystemSettings.Add(model);
            }
            else
            {
                settings.WelcomeTitle = model.WelcomeTitle;
                settings.WelcomeDescription = model.WelcomeDescription;
                settings.AdminWelcome = model.AdminWelcome;
                settings.AdminDescription = model.AdminDescription;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "تم حفظ الإعدادات بنجاح";

            return RedirectToAction(nameof(Index));
        }

        // ==================== إدارة المستخدمين ====================

        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();
            var usersWithRoles = new List<UserWithRoleViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                usersWithRoles.Add(new UserWithRoleViewModel
                {
                    User = user,
                    Roles = roles.ToList(),
                    PrimaryRole = roles.FirstOrDefault() ?? "Supervisor"
                });
            }

            ViewBag.AllRoles = SystemRoles.AllRoles;
            return View(usersWithRoles);
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        public IActionResult CreateUser()
        {
            ViewBag.AllRoles = SystemRoles.AllRoles;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> CreateUser(CreateUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.AllRoles = SystemRoles.AllRoles;
                return View(model);
            }

            var user = new ApplicationUser
            {
                FullName = model.FullName,
                UserName = model.Username,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                Department = model.Department,
                EmailConfirmed = true,
                IsActive = true,
                CustomPermissions = model.CustomPermissions
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                // إضافة الدور
                if (!string.IsNullOrEmpty(model.Role))
                {
                    await _userManager.AddToRoleAsync(user, model.Role);
                }

                TempData["Success"] = "تم إضافة المستخدم بنجاح";
                return RedirectToAction(nameof(Users));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            ViewBag.AllRoles = SystemRoles.AllRoles;
            return View(model);
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);

            var model = new EditUserViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Username = user.UserName!,
                Email = user.Email!,
                PhoneNumber = user.PhoneNumber,
                Department = user.Department,
                IsActive = user.IsActive,
                Role = roles.FirstOrDefault() ?? "Supervisor",
                CustomPermissions = user.CustomPermissions
            };

            ViewBag.AllRoles = SystemRoles.AllRoles;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> EditUser(EditUserViewModel model, string? CustomPermissionsString)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.AllRoles = SystemRoles.AllRoles;
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return NotFound();

            user.FullName = model.FullName;
            user.UserName = model.Username;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;
            user.Department = model.Department;
            user.IsActive = model.IsActive;
            
            // حفظ الصلاحيات المخصصة للمشرف
            if (model.Role == SystemRoles.Supervisor)
            {
                user.CustomPermissions = CustomPermissionsString;
            }
            else
            {
                user.CustomPermissions = null;
            }

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                // تحديث الدور
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                
                if (!string.IsNullOrEmpty(model.Role))
                {
                    await _userManager.AddToRoleAsync(user, model.Role);
                }

                // تحديث كلمة المرور إذا تم إدخالها
                if (!string.IsNullOrEmpty(model.NewPassword))
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    await _userManager.ResetPasswordAsync(user, token, model.NewPassword);
                }

                TempData["Success"] = "تم تحديث بيانات الم��تخدم بنجاح";
                return RedirectToAction(nameof(Users));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            ViewBag.AllRoles = SystemRoles.AllRoles;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> ToggleUserStatus(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                user.IsActive = !user.IsActive;
                await _userManager.UpdateAsync(user);
                TempData["Success"] = user.IsActive ? "تم تفعيل المستخدم" : "تم إيقاف المستخدم";
            }

            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.Id == userId)
            {
                TempData["Error"] = "لا يمكنك حذف حسابك الخاص";
                return RedirectToAction(nameof(Users));
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
                TempData["Success"] = "تم حذف المستخدم";
            }

            return RedirectToAction(nameof(Users));
        }

        // ==================== عرض الصلاحيات ====================

        [Authorize(Roles = "SuperAdmin,Admin")]
        public IActionResult PermissionsMatrix()
        {
            var rolesWithPermissions = new Dictionary<string, List<string>>();
            
            foreach (var role in SystemRoles.AllRoles)
            {
                rolesWithPermissions[role] = Models.Permissions.RolePermissions.GetValueOrDefault(role, new List<string>());
            }

            return View("Permissions", rolesWithPermissions);
        }
    }

    // ==================== ViewModels ====================

    public class UserWithRoleViewModel
    {
        public ApplicationUser User { get; set; } = null!;
        public List<string> Roles { get; set; } = new();
        public string PrimaryRole { get; set; } = string.Empty;
    }

    public class CreateUserViewModel
    {
        [Required(ErrorMessage = "الاسم الكامل مطلوب")]
        [Display(Name = "الاسم الكامل")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "اسم المستخدم مطلوب")]
        [Display(Name = "اسم المستخدم")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة")]
        [Display(Name = "البريد الإلكتروني")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        [MinLength(6, ErrorMessage = "كلمة المرور يجب أن تكون 6 أحرف على الأقل")]
        [Display(Name = "كلمة المرور")]
        public string Password { get; set; } = string.Empty;

        [Compare("Password", ErrorMessage = "كلمتا المرور غير متطابقتين")]
        [Display(Name = "تأكيد كلمة المرور")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Display(Name = "رقم الهاتف")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "القسم")]
        public string? Department { get; set; }

        [Required(ErrorMessage = "الدور مطلوب")]
        [Display(Name = "الدور")]
        public string Role { get; set; } = "Supervisor";

        [Display(Name = "الصلاحيات المخصصة")]
        public string? CustomPermissions { get; set; }
    }

    public class EditUserViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "الاسم الكامل مطلوب")]
        [Display(Name = "الاسم الكامل")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "اسم المستخدم مطلوب")]
        [Display(Name = "اسم المستخدم")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة")]
        [Display(Name = "البريد الإلكتروني")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "كلمة المرور الجديدة")]
        [MinLength(6, ErrorMessage = "كلمة المرور يجب أن تكون 6 أحرف على الأقل")]
        public string? NewPassword { get; set; }

        [Display(Name = "رقم الهاتف")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "القسم")]
        public string? Department { get; set; }

        [Required(ErrorMessage = "الدور مطلوب")]
        [Display(Name = "الدور")]
        public string Role { get; set; } = "Supervisor";

        [Display(Name = "نشط")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "الصلاحيات المخصصة")]
        public string? CustomPermissions { get; set; }
    }
}
