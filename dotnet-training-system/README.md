# نظام إدارة البرامج التدريبية
## Training Programs Management System - ASP.NET Core 9 MVC

### المتطلبات
- .NET 9 SDK
- SQL Server (LocalDB أو أي إصدار آخر)
- Visual Studio 2022 أو VS Code

### خطوات التشغيل

#### 1. استنساخ المشروع
```bash
cd dotnet-training-system
```

#### 2. استعادة الحزم
```bash
dotnet restore
```

#### 3. إنشاء قاعدة البيانات
```bash
cd TrainingSystem
dotnet ef migrations add InitialCreate
dotnet ef database update
```

#### 4. تشغيل المشروع
```bash
dotnet run
```

أو من Visual Studio: اضغط F5

### بيانات الدخول الافتراضية
- **اسم المستخدم:** admin
- **كلمة المرور:** Admin@123

### هيكل المشروع

```
TrainingSystem/
├── Controllers/
│   ├── HomeController.cs          # الصفحات العامة
│   ├── AccountController.cs       # تسجيل الدخول
│   ├── AdminController.cs         # لوحة التحكم
│   ├── ProgramsController.cs      # إدارة البرامج
│   ├── BatchesController.cs       # إدارة الدفعات
│   ├── RegistrationsController.cs # طلبات الترشيح
│   └── SettingsController.cs      # الإعدادات
├── Models/
│   ├── TrainingProgram.cs         # نموذج البرنامج
│   ├── Batch.cs                   # نموذج الدفعة
│   ├── Registration.cs            # نموذج التسجيل
│   ├── ApplicationUser.cs         # نموذج المستخدم
│   └── SystemSettings.cs          # إعدادات النظام
├── ViewModels/
│   ├── DashboardViewModel.cs      # نماذج العرض
│   └── LoginViewModel.cs          # نماذج تسجيل الدخول
├── Data/
│   └── ApplicationDbContext.cs    # سياق قاعدة البيانات
├── Views/
│   ├── Home/                      # صفحات الموظفين
│   ├── Account/                   # صفحات الدخول
│   ├── Admin/                     # لوحة التحكم
│   ├── Programs/                  # إدارة البرامج
│   ├── Batches/                   # إدارة الدفعات
│   ├── Registrations/             # طلبات الترشيح
│   ├── Settings/                  # الإعدادات
│   └── Shared/                    # القوالب المشتركة
└── wwwroot/
    └── css/
        └── site.css               # التنسيقات
```

### الميزات

#### للموظفين:
- استعراض البرامج التدريبية المتاحة
- البحث والفلترة
- عرض تفاصيل البرنامج
- التسجيل في الدفعات المتاحة

#### لمدير النظام:
- لوحة تحكم شاملة بالإحصائيات
- إدارة البرامج التدريبية (إضافة/تعديل/حذف)
- إدارة الدفعات
- مراجعة طلبات الترشيح (قبول/رفض)
- تصدير التقارير إلى Excel
- إدارة المستخدمين
- تخصيص إعدادات النظام

### تغيير قاعدة البيانات

لتغيير قاعدة البيانات، عدّل `ConnectionStrings` في ملف `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=TrainingSystemDb;User Id=YOUR_USER;Password=YOUR_PASSWORD;"
  }
}
```

### النشر على IIS

1. نشر المشروع:
```bash
dotnet publish -c Release -o ./publish
```

2. إنشاء موقع جديد في IIS
3. توجيه المسار إلى مجلد `publish`
4. تأكد من تثبيت ASP.NET Core Hosting Bundle

### الدعم الفني
للاستفسارات أو المشاكل التقنية، يرجى التواصل مع فريق تقنية المعلومات.
