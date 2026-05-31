using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.Data;
using TrainingSystem.Models;
using TrainingSystem.ViewModels;

namespace TrainingSystem.Controllers
{
    [Authorize]
    [Route("Admin/[controller]")]
    public class SettingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SettingsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // صفحة الإعدادات
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var settings = await _context.SystemSettings.FirstOrDefaultAsync() ?? new SystemSettings();
            var users = await _userManager.Users.ToListAsync();

            ViewBag.Users = users;
            return View(settings);
        }

        // حفظ إعدادات الترحيب
        [HttpPost("UpdateWelcome")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateWelcome(SystemSettings model)
        {
            var settings = await _context.SystemSettings.FirstOrDefaultAsync();
            
            if (settings == null)
            {
                settings = new SystemSettings();
                _context.SystemSettings.Add(settings);
            }

            settings.WelcomeTitle = model.WelcomeTitle;
            settings.WelcomeDescription = model.WelcomeDescription;
            settings.AdminWelcome = model.AdminWelcome;
            settings.AdminDescription = model.AdminDescription;
            settings.OrganizationName = model.OrganizationName;
            settings.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Success"] = "تم حفظ إعدادات الترحيب بنجاح";
            return RedirectToAction(nameof(Index));
        }

        // إضافة مستخدم جديد
        [HttpGet("Users/Create")]
        public IActionResult CreateUser()
        {
            return View(new UserViewModel());
        }

        [HttpPost("Users/Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(UserViewModel model)
        {
            if (string.IsNullOrEmpty(model.Password))
            {
                ModelState.AddModelError("Password", "كلمة المرور مطلوبة");
            }

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    FullName = model.FullName,
                    Role = model.Role,
                    IsActive = model.IsActive,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, model.Password!);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, model.Role.ToString());
                    TempData["Success"] = "تم إضافة المستخدم بنجاح";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }

        // تعديل مستخدم
        [HttpGet("Users/Edit/{id}")]
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var model = new UserViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                UserName = user.UserName!,
                Email = user.Email!,
                Role = user.Role,
                IsActive = user.IsActive
            };

            return View(model);
        }

        [HttpPost("Users/Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(string id, UserViewModel model)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // إزالة التحقق من كلمة المرور لأنها اختيارية عند التعديل
            ModelState.Remove("Password");

            if (ModelState.IsValid)
            {
                user.FullName = model.FullName;
                user.UserName = model.UserName;
                user.Email = model.Email;
                user.Role = model.Role;
                user.IsActive = model.IsActive;

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    // تحديث كلمة المرور إذا تم إدخالها
                    if (!string.IsNullOrEmpty(model.Password))
                    {
                        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                        await _userManager.ResetPasswordAsync(user, token, model.Password);
                    }

                    // تحديث الأدوار
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    await _userManager.AddToRoleAsync(user, model.Role.ToString());

                    TempData["Success"] = "تم تحديث المستخدم بنجاح";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }

        // حذف مستخدم
        [HttpPost("Users/Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // منع حذف المستخدم الحالي
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.Id == id)
            {
                TempData["Error"] = "لا يمكنك حذف حسابك الحالي";
                return RedirectToAction(nameof(Index));
            }

            await _userManager.DeleteAsync(user);
            TempData["Success"] = "تم حذف المستخدم بنجاح";
            return RedirectToAction(nameof(Index));
        }
    }
}
