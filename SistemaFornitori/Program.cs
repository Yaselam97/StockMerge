using Microsoft.EntityFrameworkCore;
using SistemaFornitori.Data;
using SistemaFornitori.Services;

var builder = WebApplication.CreateBuilder(args);

// MVC
builder.Services.AddControllersWithViews();

// EF Core — DB interno SistemaFornitori
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SistemaFornitori")));

// Services
var mappingsPath = Path.Combine(builder.Environment.ContentRootPath, "Mappings");
builder.Services.AddSingleton(new MappingService(mappingsPath));
builder.Services.AddScoped<ImportService>();
builder.Services.AddScoped<InterscambioService>();
builder.Services.AddScoped<ExportService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// Route di default: apre la pagina Import
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Import}/{action=Index}/{id?}");

app.Run();