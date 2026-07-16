-- =============================================================
--  سكربت إضافة الأعمدة الجديدة إلى قاعدة البيانات القديمة
--  آمن تماماً: يحافظ على كل بياناتك الموجودة
--  يمكن تشغيله أكثر من مرة بدون خطأ (يتحقق قبل الإضافة)
-- =============================================================
--  الأعمدة المُضافة:
--    1) Registrations.JobTitle       (المسمى الوظيفي)
--    2) TrainingPrograms.ProgramCode  (رمز البرنامج)
--    3) TrainingPrograms.ReferenceNumber (الرقم المرجعي)
-- =============================================================

-- 1) المسمى الوظيفي في جدول الطلبات (مطلوب في النموذج)
--    نضيفه بقيمة افتراضية فارغة حتى لا تفشل الإضافة على الصفوف الموجودة
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE Name = N'JobTitle'
      AND Object_ID = Object_ID(N'dbo.Registrations')
)
BEGIN
    ALTER TABLE dbo.Registrations
        ADD JobTitle NVARCHAR(MAX) NOT NULL DEFAULT N'';
    PRINT 'تمت إضافة العمود: Registrations.JobTitle';
END
ELSE
    PRINT 'العمود موجود مسبقاً: Registrations.JobTitle';
GO

-- 2) رمز البرنامج في جدول البرامج (اختياري)
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE Name = N'ProgramCode'
      AND Object_ID = Object_ID(N'dbo.TrainingPrograms')
)
BEGIN
    ALTER TABLE dbo.TrainingPrograms
        ADD ProgramCode NVARCHAR(MAX) NULL;
    PRINT 'تمت إضافة العمود: TrainingPrograms.ProgramCode';
END
ELSE
    PRINT 'العمود موجود مسبقاً: TrainingPrograms.ProgramCode';
GO

-- 3) الرقم المرجعي في جدول البرامج (اختياري)
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE Name = N'ReferenceNumber'
      AND Object_ID = Object_ID(N'dbo.TrainingPrograms')
)
BEGIN
    ALTER TABLE dbo.TrainingPrograms
        ADD ReferenceNumber NVARCHAR(MAX) NULL;
    PRINT 'تمت إضافة العمود: TrainingPrograms.ReferenceNumber';
END
ELSE
    PRINT 'العمود موجود مسبقاً: TrainingPrograms.ReferenceNumber';
GO

PRINT '=== اكتمل التحديث بنجاح ===';
GO
