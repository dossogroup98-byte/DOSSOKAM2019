using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OfficeOpenXml;
using DOSSOKAM2019.Data;
using Npgsql;

// EPPlus lisansı
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// SQL Server Connection (Mevcut)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// PostgreSQL Connection (Yeni Eklendi)
builder.Services.AddDbContext<PostgreSQLDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQLConnection")));

var app = builder.Build();

// PostgreSQL connection test
try
{
    using var scope = app.Services.CreateScope();
    var postgresContext = scope.ServiceProvider.GetRequiredService<PostgreSQLDbContext>();
    await postgresContext.Database.CanConnectAsync();
    Console.WriteLine("✅ PostgreSQL bağlantısı başarılı!");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ PostgreSQL bağlantı hatası: {ex.Message}");
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // PWA için gerekli
app.UseRouting();
app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();