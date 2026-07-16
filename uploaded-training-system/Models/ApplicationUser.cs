using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace TrainingSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [Display(Name = "الاسم الكامل")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "نشط")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "تاريخ الإنشاء")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "آخر تسجيل دخول")]
        public DateTime? LastLoginAt { get; set; }

        [Display(Name = "الصورة الشخصية")]
        public string? AvatarUrl { get; set; }

        [Display(Name = "رقم الهاتف")]
        public new string? PhoneNumber { get; set; }

        [Display(Name = "القسم")]
        public string? Department { get; set; }

        // صلاحيات مخصصة للمشرف (يحددها المدير)
        public string? CustomPermissions { get; set; }
    }

    // الصلاحيات المتاحة في النظام
    public static class SystemRoles
    {
        public const string SuperAdmin = "SuperAdmin";  // مدير النظام - كل الصلاحيات
        public const string Admin = "Admin";            // مدير - كل الصلاحيات (مثل مدير النظام)
        public const string Supervisor = "Supervisor";  // مشرف - صلاحيات يحددها المدير

        public static List<string> AllRoles => new() { SuperAdmin, Admin, Supervisor };

        public static string GetRoleDisplayName(string role)
        {
            return role switch
            {
                SuperAdmin => "مدير النظام",
                Admin => "مدير",
                Supervisor => "مشرف",
                _ => role
            };
        }

        public static string GetRoleDescription(string role)
        {
            return role switch
            {
                SuperAdmin => "كل الصلاحيات - إدارة المستخدمين والإعدادات والبرامج",
                Admin => "كل الصلاحيات - إدارة المستخدمين والإعدادات والبرامج",
                Supervisor => "صلاحيات يحددها مدير النظام أو المدير",
                _ => ""
            };
        }
    }

    // صلاحيات محددة للتحكم الدقيق
    public static class Permissions
    {
        // البرامج التدريبية
        public const string Programs_View = "Programs.View";
        public const string Programs_Create = "Programs.Create";
        public const string Programs_Edit = "Programs.Edit";
        public const string Programs_Delete = "Programs.Delete";

        // الدفعات
        public const string Batches_View = "Batches.View";
        public const string Batches_Create = "Batches.Create";
        public const string Batches_Edit = "Batches.Edit";
        public const string Batches_Delete = "Batches.Delete";

        // التسجيلات
        public const string Registrations_View = "Registrations.View";
        public const string Registrations_Approve = "Registrations.Approve";
        public const string Registrations_Reject = "Registrations.Reject";
        public const string Registrations_Delete = "Registrations.Delete";
        public const string Registrations_Export = "Registrations.Export";

        // المستخدمين
        public const string Users_View = "Users.View";
        public const string Users_Create = "Users.Create";
        public const string Users_Edit = "Users.Edit";
        public const string Users_Delete = "Users.Delete";

        // الإعدادات
        public const string Settings_View = "Settings.View";
        public const string Settings_Edit = "Settings.Edit";

        // التقارير
        public const string Reports_View = "Reports.View";
        public const string Reports_Export = "Reports.Export";

        // جميع الصلاحيات المتاحة
        public static List<string> AllPermissions => new()
        {
            Programs_View, Programs_Create, Programs_Edit, Programs_Delete,
            Batches_View, Batches_Create, Batches_Edit, Batches_Delete,
            Registrations_View, Registrations_Approve, Registrations_Reject, Registrations_Delete, Registrations_Export,
            Users_View, Users_Create, Users_Edit, Users_Delete,
            Settings_View, Settings_Edit,
            Reports_View, Reports_Export
        };

        // أسماء الصلاحيات بالعربي
        public static string GetPermissionDisplayName(string permission)
        {
            return permission switch
            {
                Programs_View => "عرض البرامج",
                Programs_Create => "إضافة البرامج",
                Programs_Edit => "تعديل البرامج",
                Programs_Delete => "حذف البرامج",
                Batches_View => "عرض الدفعات",
                Batches_Create => "إضافة الدفعات",
                Batches_Edit => "تعديل الدفعات",
                Batches_Delete => "حذف الدفعات",
                Registrations_View => "عرض طلبات الترشيح",
                Registrations_Approve => "قبول طلبات الترشيح",
                Registrations_Reject => "رفض طلبات الترشيح",
                Registrations_Delete => "حذف طلبات الترشيح",
                Registrations_Export => "تصدير طلبات الترشيح",
                Users_View => "عرض المستخدمين",
                Users_Create => "إضافة المستخدمين",
                Users_Edit => "تعديل المستخدمين",
                Users_Delete => "حذف المستخدمين",
                Settings_View => "عرض الإعدادات",
                Settings_Edit => "تعديل الإعدادات",
                Reports_View => "عرض التقارير",
                Reports_Export => "تصدير التقارير",
                _ => permission
            };
        }

        // صلاحيات كل دور
        public static Dictionary<string, List<string>> RolePermissions => new()
        {
            {
                SystemRoles.SuperAdmin, AllPermissions
            },
            {
                SystemRoles.Admin, AllPermissions
            },
            {
                SystemRoles.Supervisor, new List<string>
                {
                    // الصلاحيات الافتراضية للمشرف (يمكن تعديلها من قبل المدير)
                    Programs_View,
                    Batches_View,
                    Registrations_View,
                    Reports_View
                }
            }
        };

        public static bool HasPermission(string role, string permission, string? customPermissions = null)
        {
            // مدير النظام والمدير لهم كل الصلاحيات
            if (role == SystemRoles.SuperAdmin || role == SystemRoles.Admin)
            {
                return true;
            }

            // للمشرف: تحقق من الصلاحيات المخصصة أولاً
            if (role == SystemRoles.Supervisor && !string.IsNullOrEmpty(customPermissions))
            {
                var userPermissions = customPermissions.Split(',').ToList();
                return userPermissions.Contains(permission);
            }

            // الصلاحيات الافتراضية
            if (RolePermissions.TryGetValue(role, out var permissions))
            {
                return permissions.Contains(permission);
            }
            return false;
        }
    }
}
