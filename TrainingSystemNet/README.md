# نظام إدارة البرامج التدريبية
## Training Programs Management System

نظام متكامل لإدارة البرامج التدريبية مبني بتقنية ASP.NET Core MVC 9

---

## المتطلبات

- .NET 9 SDK
- SQL Server (LocalDB أو Express أو Full)
- Visual Studio 2022 (اختياري)

---

## خطوات التشغيل

### 1. فتح المشروع
```
افتح ملف TrainingSystem.sln في Visual Studio
أو استخدم Command Prompt
```

### 2. تثبيت المكتبات
```cmd
cd TrainingSystem
dotnet restore
```

### 3. إنشاء قاعدة البيانات
```cmd
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 4. تشغيل التطبيق
```cmd
dotnet run
```

### 5. فتح المتصفح
```
https://localhost:5001
```

---

## بيانات الدخول الافتراضية

| المستخدم | كلمة المرور | الدور |
|----------|-------------|-------|
| admin | Admin@123 | SuperAdmin (مدير النظام) |

---

## نظام الصلاحيات

| الصفحة | SuperAdmin | Admin | Supervisor | Viewer |
|--------|:----------:|:-----:|:----------:|:------:|
| لوحة التحكم | ✓ | ✓ | ✓ | ✓ |
| عرض البرامج | ✓ | ✓ | ✓ | ✓ |
| إضافة/تعديل/حذف برنامج | ✓ | ✓ | ✗ | ✗ |
| عرض الدفعات | ✓ | ✓ | ✓ | ✓ |
| إضافة/تعديل/حذف دفعة | ✓ | ✓ | ✗ | ✗ |
| عرض طلبات الترشيح | ✓ | ✓ | ✓ | ✓ |
| قبول/رفض طلب | ✓ | ✓ | ✓ | ✗ |
| حذف طلب | ✓ | ✓ | ✗ | ✗ |
| الإعدادات | ✓ | ✓ | ✗ | ✗ |
| إدارة المستخدمين | ✓ | ✗ | ✗ | ✗ |
| التقارير | ✓ | ✓ | ✓ | ✓ |

---

## هيكل المشروع

```
TrainingSystem/
├── Controllers/           # المتحكمات
│   ├── AccountController.cs      # تسجيل الدخول
│   ├── AdminController.cs        # لوحة التحكم
│   ├── BatchesController.cs      # إدارة الدفعات
│   ├── HomeController.cs         # الصفحة الرئيسية
│   ├── ProgramsController.cs     # إدارة البرامج
│   ├── RegistrationsController.cs # طلبات الترشيح
│   └── SettingsController.cs     # الإعدادات والمستخدمين
│
├── Models/                # النماذج
│   ├── ApplicationUser.cs        # المستخدم
│   ├── Batch.cs                  # الدفعة
│   ├── Registration.cs           # طلب الترشيح
│   ├── SystemSettings.cs         # إعدادات النظام
│   └── TrainingProgram.cs        # البرنامج التدريبي
│
├── Data/                  # قاعدة البيانات
│   └── ApplicationDbContext.cs
│
├── Views/                 # الصفحات
│   ├── Account/          # صفحات الحساب
│   ├── Admin/            # لوحة التحكم
│   ├── Batches/          # صفحات الدفعات
│   ├── Home/             # الصفحة الرئيسية
│   ├── Programs/         # صفحات البرامج
│   ├── Registrations/    # صفحات الترشيح
│   ├── Settings/         # صفحات الإعدادات
│   └── Shared/           # القوالب المشتركة
│
├── wwwroot/css/          # ملفات CSS
├── Program.cs            # نقطة البداية
├── appsettings.json      # الإعدادات
└── TrainingSystem.csproj # ملف المشروع
```

---

## حل المشاكل الشائعة

### مشكلة Foreign Key
```cmd
rd /s /q Migrations
dotnet ef database drop --force
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### مشكلة المكتبات
```cmd
dotnet restore
dotnet build
```

---

## الدعم

للمساعدة أو الاستفسارات، تواصل مع فريق التطوير.
