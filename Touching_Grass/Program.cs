using Touching_Grass.Repositories;
using Touching_Grass.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddSingleton<JsonDataLoader>();
builder.Services.AddSingleton<IZipLookupService, ZipLookupService>();
builder.Services.AddSingleton<IPlantRepository, PlantRepository>();
builder.Services.AddSingleton<IWateringCalculatorService, WateringCalculatorService>();

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

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
