@echo off
echo ========================================
echo حذف قاعدة البيانات والـ Migrations
echo ========================================

echo.
echo [1/5] حذف مجلد Migrations...
if exist "Migrations" rd /s /q "Migrations"

echo [2/5] حذف مجلد bin و obj...
if exist "bin" rd /s /q "bin"
if exist "obj" rd /s /q "obj"

echo [3/5] حذف قاعدة البيانات...
dotnet ef database drop --force

echo [4/5] إعادة بناء المشروع...
dotnet restore
dotnet build

echo [5/5] إنشاء Migration جديد...
dotnet ef migrations add InitialCreate
dotnet ef database update

echo.
echo ========================================
echo تم بنجاح! الآن شغّل: dotnet run
echo ========================================
pause
