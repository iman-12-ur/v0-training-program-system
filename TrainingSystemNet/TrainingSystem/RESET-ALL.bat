@echo off
chcp 65001 >nul
echo ========================================
echo   اعادة تعيين كاملة لقاعدة البيانات
echo ========================================
echo.

echo [1/7] حذف مجلد Migrations...
if exist Migrations rd /s /q Migrations
echo تم الحذف
echo.

echo [2/7] حذف مجلد bin...
if exist bin rd /s /q bin
echo تم الحذف
echo.

echo [3/7] حذف مجلد obj...
if exist obj rd /s /q obj
echo تم الحذف
echo.

echo [4/7] حذف قاعدة البيانات...
dotnet ef database drop --force
echo.

echo [5/7] استعادة الحزم...
dotnet restore
echo.

echo [6/7] انشاء Migration جديد...
dotnet ef migrations add InitialCreate
echo.

echo [7/7] تحديث قاعدة البيانات...
dotnet ef database update
echo.

echo ========================================
echo   تم بنجاح! الان شغل: dotnet run
echo ========================================
pause
