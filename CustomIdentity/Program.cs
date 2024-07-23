using CustomIdentity;
using CustomIdentity.Data;
using CustomIdentity.Models;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient<SparrowSmsService>();

// Register Application Context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});



// Add Identity services
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequiredUniqueChars = 0;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure ApplicationCookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.AccessDeniedPath = "/Home/AccessDenied";
});

// Configure Hangfire to use PostgreSQL
builder.Services.AddHangfire(configuration =>
    configuration.UsePostgreSqlStorage(builder.Configuration.GetConnectionString("DefaultConnection"), new PostgreSqlStorageOptions { }));

builder.Services.AddHangfireServer();

// Register the RoleSeeder service
builder.Services.AddScoped<RoleSeeder>();

// Register the SmsService
builder.Services.AddTransient<SparrowSmsService>();

var app = builder.Build();

// Seed roles on application startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleSeeder = services.GetRequiredService<RoleSeeder>();
    await roleSeeder.SeedRolesAsync();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseHangfireDashboard(); // Enable the Hangfire dashboard

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
// Schedule the SMS sending job every weekday at 5:15 AM UTC (which is 11 AM local time in UTC+5:45)
RecurringJob.AddOrUpdate<SmsController>(
    "send-sms-job",
    controller => controller.SendScheduledSms(),
    "15 4 * * 1-5"); // Cron expression for weekdays at 5:15 AM UTC


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
