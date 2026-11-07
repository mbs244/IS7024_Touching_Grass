using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Touching_Grass.Data;
using Touching_Grass.Models;
using Touching_Grass.Repositories;
using Touching_Grass.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    connectionString = builder.Configuration["AZURE_SQL_CONNECTIONSTRING"];
}

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "A database connection string is required. Configure ConnectionStrings:DefaultConnection or AZURE_SQL_CONNECTIONSTRING.");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequiredLength = 8;
        options.Password.RequiredUniqueChars = 1;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddRazorPages();
builder.Services.AddSingleton<JsonDataLoader>();
builder.Services.AddSingleton<IZipLookupService, ZipLookupService>();
builder.Services.AddSingleton<IPlantRepository, PlantRepository>();
builder.Services.AddSingleton<IWateringCalculatorService, WateringCalculatorService>();
builder.Services.AddScoped<IUserPlantService, UserPlantService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
