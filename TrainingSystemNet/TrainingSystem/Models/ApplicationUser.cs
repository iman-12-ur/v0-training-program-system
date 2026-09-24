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

        // --- حقول الموظف (لوحدة تحليل الاحتياجات التدريبية) - كلها اختيارية ---

        [Display(Name = "الرقم الوظيفي")]
        [MaxLength(50)]
        public string? EmployeeNumber { get; set; }

        [Display(Name = "المسمى الوظيفي")]
        [MaxLength(150)]
        public string? JobTitle { get; set; }

        [Display(Name = "الدرجة الوظيفية")]
        [MaxLength(50)]
        public string? Grade { get; set; }

        [Display(Name = "المدير المباشر")]
        [MaxLength(450)]
        public string? ManagerUserId { get; set; }
    }

    // الصلاحيات المتاحة في النظام
    public static class SystemRoles
    {
        public const string SuperAdmin = "SuperAdmin";  // مدير النظام - كل الصلاحيات
        public const string Admin = "Admin";            // دائرة التدريب - المراجعة والاعتماد النهائي
        public const string DepartmentManager = "DepartmentManager"; // مدير الدائرة - يعتمد ما يرفعه رؤساء الأقسام
        public const string SectionHead = "SectionHead";             // رئيس القسم - يجمع احتياجات قسمه ويرفعها
        public const string Supervisor = "Supervisor";  // مشرف - صلاحيات يحددها المدير (متوافقية مع النظام الحالي)

        public static List<string> AllRoles => new() { SuperAdmin, Admin, DepartmentManager, SectionHead, Supervisor };

        public static string GetRoleDisplayName(string role)
        {
            return role switch
            {
                SuperAdmin => "مدير النظام",
                Admin => "دائرة التدريب",
                DepartmentManager => "مدير الدائرة",
                SectionHead => "رئيس القسم",
                Supervisor => "مشرف",
                _ => role
            };
        }

        public static string GetRoleDescription(string role)
        {
            return role switch
            {
                SuperAdmin => "كل الصلاحيات - إدارة المستخدمين والإعدادات والبرامج",
                Admin => "دائرة التدريب - مراجعة الاحتياجات واعتمادها نهائياً وإدارة البرامج",
                DepartmentManager => "مدير الدائرة - يعتمد الاحتياجات المرفوعة من رؤساء الأقسام في دائرته",
                SectionHead => "رئيس القسم - يجمع احتياجات موظفي قسمه وينشئها ويرفعها للاعتماد",
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

        // --- صلاحيات وحدة الاحتياجات التدريبية (TNA) ---
        // تُضاف ضمن نظام الصلاحيات القائم (Roles → Permissions → Users) وتظهر في شاشة إدارة الصلاحيات
        // ليتمكن مدير النظام من منحها/سحبها لأي مستخدم دون إنشاء نظام صلاحيات منفصل.
        public const string TrainingNeeds_View = "TrainingNeeds.View";
        public const string TrainingNeeds_Create = "TrainingNeeds.Create";
        public const string TrainingNeeds_Edit = "TrainingNeeds.Edit";
        public const string TrainingNeeds_Submit = "TrainingNeeds.Submit";
        public const string TrainingNeeds_Review = "TrainingNeeds.Review";
        public const string TrainingNeeds_Approve = "TrainingNeeds.Approve";
        public const string TrainingNeeds_Reject = "TrainingNeeds.Reject";
        public const string TrainingNeeds_Return = "TrainingNeeds.Return";
        public const string TrainingNeeds_Manage = "TrainingNeeds.Manage";
        public const string TrainingNeeds_Budget = "TrainingNeeds.Budget";
        public const string TrainingNeeds_ImpactAssessment = "TrainingNeeds.ImpactAssessment";
        public const string TrainingNeeds_Reports = "TrainingNeeds.Reports";
        public const string TrainingNeeds_Export = "TrainingNeeds.Export";

        // جميع الصلاحيات المتاحة
        public static List<string> AllPermissions => new()
        {
            Programs_View, Programs_Create, Programs_Edit, Programs_Delete,
            Batches_View, Batches_Create, Batches_Edit, Batches_Delete,
            Registrations_View, Registrations_Approve, Registrations_Reject, Registrations_Delete, Registrations_Export,
            Users_View, Users_Create, Users_Edit, Users_Delete,
            Settings_View, Settings_Edit,
            Reports_View, Reports_Export,
            // صلاحيات وحدة TNA
            TrainingNeeds_View, TrainingNeeds_Create, TrainingNeeds_Edit, TrainingNeeds_Submit,
            TrainingNeeds_Review, TrainingNeeds_Approve, TrainingNeeds_Reject, TrainingNeeds_Return,
            TrainingNeeds_Manage, TrainingNeeds_Budget, TrainingNeeds_ImpactAssessment,
            TrainingNeeds_Reports, TrainingNeeds_Export
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
                TrainingNeeds_View => "عرض الاحتياجات التدريبية",
                TrainingNeeds_Create => "إنشاء احتياج تدريبي",
                TrainingNeeds_Edit => "تعديل الاحتياج التدريبي",
                TrainingNeeds_Submit => "رفع الاحتياج للاعتماد",
                TrainingNeeds_Review => "مراجعة الاحتياجات التدريبية",
                TrainingNeeds_Approve => "اعتماد الاحتياج التدريبي",
                TrainingNeeds_Reject => "رفض الاحتياج التدريبي",
                TrainingNeeds_Return => "إعادة الاحتياج للتعديل",
                TrainingNeeds_Manage => "إدارة وحدة الاحتياجات التدريبية",
                TrainingNeeds_Budget => "مراجعة ميزانية التدريب",
                TrainingNeeds_ImpactAssessment => "تقييم أثر التدريب",
                TrainingNeeds_Reports => "تقارير الاحتياجات التدريبية",
                TrainingNeeds_Export => "تصدير الاحتياجات التدريبية",
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
                    Reports_View,
                    TrainingNeeds_View
                }
            },
            {
                // مدير الدائرة: مراجعة واعتماد/رفض/إعادة الاحتياجات المرفوعة إليه
                SystemRoles.DepartmentManager, new List<string>
                {
                    Programs_View,
                    Batches_View,
                    Registrations_View,
                    Reports_View,
                    TrainingNeeds_View,
                    TrainingNeeds_Review,
                    TrainingNeeds_Approve,
                    TrainingNeeds_Reject,
                    TrainingNeeds_Return,
                    TrainingNeeds_Reports,
                    TrainingNeeds_Export
                }
            },
            {
                // رئيس القسم: عرض وإنشاء وتعديل ورفع احتياجات موظفي قسمه
                SystemRoles.SectionHead, new List<string>
                {
                    Programs_View,
                    Batches_View,
                    Registrations_View,
                    Reports_View,
                    TrainingNeeds_View,
                    TrainingNeeds_Create,
                    TrainingNeeds_Edit,
                    TrainingNeeds_Submit,
                    TrainingNeeds_Reports,
                    TrainingNeeds_Export
                }
            }
        };

        public static bool HasPermission(string role, string permission, string? customPermissions = null)
        {
            // مدير النظام ومسؤول النظام (دائرة التدريب) لهما كل الصلاحيات — لا تُقلَّص بإضافة TNA
            if (role == SystemRoles.SuperAdmin || role == SystemRoles.Admin)
            {
                return true;
            }

            var custom = string.IsNullOrEmpty(customPermissions)
                ? new List<string>()
                : customPermissions.Split(',').Select(p => p.Trim()).Where(p => p.Length > 0).ToList();

            // للمشرف: الصلاحيات المخصصة تُحدِّد وصوله (توافق مع السلوك الحالي)
            if (role == SystemRoles.Supervisor && custom.Count > 0)
            {
                return custom.Contains(permission);
            }

            var hasDefault = RolePermissions.TryGetValue(role, out var permissions) && permissions.Contains(permission);

            // مدير الدائرة ورئيس القسم: صلاحيات TNA قابلة للمنح/السحب من مدير النظام
            // (اتحاد الافتراضي مع المخصص) دون المساس بسلوك بقية الأدوار
            if (role == SystemRoles.DepartmentManager || role == SystemRoles.SectionHead)
            {
                return hasDefault || custom.Contains(permission);
            }

            return hasDefault;
        }
    }
}
