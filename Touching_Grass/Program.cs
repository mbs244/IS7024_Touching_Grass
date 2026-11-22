using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Touching_Grass.Data;
using Touching_Grass.Models;
using Touching_Grass.Repositories;
using Touching_Grass.Services;

var builder = WebApplication.CreateBuilder(args);

var isDevelopment = builder.Environment.IsDevelopment();
if (isDevelopment)
{
    builder.Services.AddSingleton<LocalUserPlantService>();
    builder.Services.AddSingleton<IUserPlantService>(services =>
        services.GetRequiredService<LocalUserPlantService>());
}
else
{
    var connectionString = Environment.GetEnvironmentVariable("MYSQLCONNSTR_localdb");
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException(
            "Azure persistence mode requires the protected MYSQLCONNSTR_localdb setting.");
    }

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseMySQL(connectionString));
    builder.Services.AddDefaultIdentity<ApplicationUser>(ConfigureIdentity)
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();
    builder.Services.AddHealthChecks().AddDbContextCheck<ApplicationDbContext>();
    builder.Services.AddScoped<IUserPlantService, UserPlantService>();
}

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.SecurePolicy = isDevelopment
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
    options.Cookie.HttpOnly = true;
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/Login";
});
builder.Services.AddRazorPages();
builder.Services.AddAuthorization();
builder.Services.AddSingleton<JsonDataLoader>();
builder.Services.AddSingleton<IZipLookupService, ZipLookupService>();
builder.Services.AddSingleton<IPlantRepository, PlantRepository>();
builder.Services.AddSingleton<IWateringCalculatorService, WateringCalculatorService>();

var app = builder.Build();

if (!isDevelopment)
{
    await ApplyPendingMigrationsAsync(app);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

if (!isDevelopment)
{
    app.UseAuthentication();
    app.UseAuthorization();
}

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();
if (!isDevelopment)
{
    app.MapHealthChecks("/health");
}

app.Run();

static async Task ApplyPendingMigrationsAsync(WebApplication app)
{
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
        .CreateLogger("StartupDatabaseMigration");

    try
    {
        await db.Database.MigrateAsync();
        logger.LogInformation("Startup database migration completed successfully.");
    }
    catch (Exception)
    {
        logger.LogCritical("Startup database migration failed; application readiness is blocked.");
        throw new InvalidOperationException("The application database is unavailable.");
    }
}

static void ConfigureIdentity(IdentityOptions options)
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequiredLength = 8;
    options.Password.RequiredUniqueChars = 1;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.User.RequireUniqueEmail = true;
}
