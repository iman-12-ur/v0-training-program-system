using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.Data;
using TrainingSystem.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Configure Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure Cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Seed Database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        
        await context.Database.MigrateAsync();
        await SeedData.Initialize(context, userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

app.Run();

// Seed Data Class
public static class SeedData
{
    public static async Task Initialize(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        // Create Roles
        string[] roles = { "SuperAdmin", "Admin", "Supervisor" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Create Default Admin User
        var adminEmail = "admin@training.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = "admin",
                Email = adminEmail,
                FullName = "مدير النظام",
                Role = UserRole.SuperAdmin,
                IsActive = true,
                EmailConfirmed = true
            };
            
            var result = await userManager.CreateAsync(adminUser, "Admin@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "SuperAdmin");
            }
        }

        // Seed Sample Program if none exists
        if (!context.TrainingPrograms.Any())
        {
            var program = new TrainingProgram
            {
                Title = "القيادة الفعّالة",
                Description = "برنامج تدريبي شامل لتطوير المهارات القيادية والإدارية للمشرفين والمدراء",
                Categories = "القيادة والإدارة",
                ProgramType = "تطوير مهني",
                TargetAudience = "المشرفين والمدراء",
                Duration = "5 أيام",
                Instructor = "د. أحمد محمد",
                Location = "قاعة التدريب الرئيسية",
                Objectives = "فهم أساسيات القيادة الفعالة\nتطوير مهارات التواصل\nبناء فرق عمل متماسكة",
                Topics = "مفهوم القيادة وأنماطها\nمهارات التأثير والإقناع\nإدارة فرق العمل\nحل المشكلات واتخاذ القرارات",
                Prerequisites = "خبرة لا تقل عن سنتين\nموافقة المدير المباشر",
                Status = ProgramStatus.Active,
                CreatedAt = DateTime.Now
            };

            context.TrainingPrograms.Add(program);
            await context.SaveChangesAsync();

            // Add a sample batch
            var batch = new Batch
            {
                Name = "الدفعة الأولى",
                StartDate = DateTime.Now.AddDays(30),
                EndDate = DateTime.Now.AddDays(35),
                MaxParticipants = 25,
                CurrentParticipants = 0,
                Status = BatchStatus.Upcoming,
                TrainingProgramId = program.Id
            };

            context.Batches.Add(batch);
            await context.SaveChangesAsync();
        }
    }
}
