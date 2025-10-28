using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using OfficeOpenXml;
using DOSSOKAM2019.Services;
using DOSSOKAM2019.Models;

ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<DOSSOKAM2019.Services.SupabaseService>();
builder.Services.AddControllersWithViews();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Supabase Service
builder.Services.AddSingleton<SupabaseService>();

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
app.UseSession();

// Render için gerekli
app.Urls.Add("http://0.0.0.0:5000");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();