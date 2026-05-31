# تعليمات حل مشكلة Foreign Key

## المشكلة:
```
FK_Registrations_TrainingPrograms_TrainingProgramId on table 'Registrations' 
may cause cycles or multiple cascade paths
```

## السبب:
لديك نسخة قديمة من المشروع تحتوي على Migrations قديمة بها FK خاطئ.

---

## الحل النهائي - خطوة بخطوة:

### الخطوة 1: حمل النسخة الجديدة

1. افتح v0
2. اضغط على النقاط الثلاث (⋮) في أعلى يمين الشاشة
3. اختر **Download ZIP**
4. **احذف المجلد القديم تماما** على جهازك
5. فك ضغط الملف الجديد

### الخطوة 2: افتح Command Prompt

1. افتح مجلد المشروع: `TrainingSystemNet\TrainingSystem`
2. في شريط العنوان اكتب `cmd` واضغط Enter
3. ستفتح نافذة Command Prompt في المجلد الصحيح

### الخطوة 3: شغل سكريبت الاعادة

اكتب هذا الامر:
```
RESET-ALL.bat
```

### الخطوة 4: شغل التطبيق

```
dotnet run
```

---

## اذا لم يعمل السكريبت، نفذ الاوامر يدويا:

```cmd
rd /s /q Migrations
rd /s /q bin
rd /s /q obj
dotnet ef database drop --force
dotnet restore
dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet run
```

---

## مهم جدا:

### تاكد من وجود dotnet-ef tool:
```cmd
dotnet tool install --global dotnet-ef
```

### تاكد من اصدار .NET:
```cmd
dotnet --version
```
يجب ان يكون 9.0 او اعلى

---

## بيانات الدخول:

- **اسم المستخدم:** admin
- **كلمة المرور:** Admin@123

---

## فيديو شرح:

للاطلاع على فيديوهات شرح Entity Framework Core:

1. **القناة الرسمية لمايكروسوفت:**
   https://www.youtube.com/@dotnet

2. **شرح EF Core Migrations بالعربي:**
   https://www.youtube.com/results?search_query=entity+framework+core+migrations+بالعربي

3. **توثيق رسمي:**
   https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/

---

## اذا استمرت المشكلة:

افتح ملف `Models/Registration.cs` وتاكد ان **لا يوجد** اي سطر يحتوي على:
- `public int TrainingProgramId`
- `public TrainingProgram TrainingProgram`

يجب ان يحتوي فقط على:
- `public int BatchId`
- `public virtual Batch? Batch`
