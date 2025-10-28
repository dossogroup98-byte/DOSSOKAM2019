using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DOSSOKAM2019.Data;
using DOSSOKAM2019.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _sqlContext;
    private readonly PostgreSQLDbContext _postgresContext;

    public HomeController(ApplicationDbContext sqlContext, PostgreSQLDbContext postgresContext)
    {
        _sqlContext = sqlContext;
        _postgresContext = postgresContext;
    }

    public async Task<IActionResult> Index(string search = "", string database = "sqlserver")
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("KullaniciAdi")))
            return RedirectToAction("Login", "Account");

        var dashboard = new DashboardViewModel();
        dashboard.SelectedDatabase = database;

        try
        {
            if (database == "postgresql")
            {
                // PostgreSQL'den veri çek
                await PostgreSQLVerileriniGetir(dashboard, search);
                ViewBag.DatabaseInfo = "PostgreSQL";
            }
            else
            {
                // SQL Server'dan veri çek
                await SQLServerVerileriniGetir(dashboard, search);
                ViewBag.DatabaseInfo = "SQL Server";
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Dashboard yüklenirken hata: {ex.Message}";
        }

        return View(dashboard);
    }

    private async Task SQLServerVerileriniGetir(DashboardViewModel dashboard, string search)
    {
        // SQL Server'dan istatistikler
        dashboard.ToplamMakineSaat = await _sqlContext.MakineSaat.CountAsync();
        dashboard.ToplamEkleme = await _sqlContext.Ekleme.CountAsync();
        dashboard.ToplamDazmal = await _sqlContext.Dazmal.CountAsync();
        dashboard.ToplamKesme = await _sqlContext.Kesme.CountAsync();
        dashboard.ToplamPaketleme = await _sqlContext.Paketleme.CountAsync();
        dashboard.ToplamHazirDokuma = await _sqlContext.HazirDokuma.CountAsync();
        dashboard.ToplamHazirMatbaa = await _sqlContext.HazirMatbaa.CountAsync();

        // SQL Server'dan son kayıtlar
        dashboard.SonMakineSaatler = await _sqlContext.MakineSaat
            .OrderByDescending(x => x.KayitTarihi)
            .Take(5)
            .ToListAsync();

        dashboard.SonEklemeIsleri = await _sqlContext.Ekleme
            .OrderByDescending(x => x.KayitTarihi)
            .Take(5)
            .ToListAsync();

        // SQL Server'dan ürün durumları
        await TumUrunDurumlariniDoldur(_sqlContext, dashboard, search);
    }

    private async Task PostgreSQLVerileriniGetir(DashboardViewModel dashboard, string search)
    {
        // PostgreSQL'den istatistikler
        dashboard.ToplamMakineSaat = await _postgresContext.MakineSaat.CountAsync();
        dashboard.ToplamEkleme = await _postgresContext.Ekleme.CountAsync();
        dashboard.ToplamDazmal = await _postgresContext.Dazmal.CountAsync();
        dashboard.ToplamKesme = await _postgresContext.Kesme.CountAsync();
        dashboard.ToplamPaketleme = await _postgresContext.Paketleme.CountAsync();
        dashboard.ToplamHazirDokuma = await _postgresContext.HazirDokuma.CountAsync();
        dashboard.ToplamHazirMatbaa = await _postgresContext.HazirMatbaa.CountAsync();

        // PostgreSQL'den son kayıtlar
        dashboard.SonMakineSaatler = await _postgresContext.MakineSaat
            .OrderByDescending(x => x.KayitTarihi)
            .Take(5)
            .ToListAsync();

        dashboard.SonEklemeIsleri = await _postgresContext.Ekleme
            .OrderByDescending(x => x.KayitTarihi)
            .Take(5)
            .ToListAsync();

        // PostgreSQL'den ürün durumları
        await TumUrunDurumlariniDoldur(_postgresContext, dashboard, search);
    }

    private async Task TumUrunDurumlariniDoldur(DbContext context, DashboardViewModel dashboard, string search)
    {
        // Generic method - hem SQL Server hem PostgreSQL için çalışır
        var makineSaatSet = context.Set<MakineSaat>();
        var eklemeSet = context.Set<Ekleme>();
        var dazmalSet = context.Set<Dazmal>();
        var kesmeSet = context.Set<Kesme>();
        var paketlemeSet = context.Set<Paketleme>();
        var hazirDokumaSet = context.Set<HazirDokuma>();
        var hazirMatbaaSet = context.Set<HazirMatbaa>();

        var makineSaatUrunler = await makineSaatSet
            .Where(x => string.IsNullOrEmpty(search) ||
                       x.SiparisNo.Contains(search) ||
                       x.UrunAdi.Contains(search))
            .Select(x => new UrunDurumu
            {
                SiparisNo = x.SiparisNo,
                UrunAdi = x.UrunAdi,
                MevcutBolum = "Makine Saat",
                Durum = x.Tamamlandi ? "Tamamlandı" : "Devam Ediyor",
                BaslamaTarihi = x.BaslamaTarihi,
                RenkKodu = x.Tamamlandi ? "success" : "danger"
            })
            .ToListAsync();

        var eklemeUrunler = await eklemeSet
            .Where(x => string.IsNullOrEmpty(search) ||
                       x.SiparisNo.Contains(search) ||
                       x.UrunAdi.Contains(search))
            .Select(x => new UrunDurumu
            {
                SiparisNo = x.SiparisNo,
                UrunAdi = x.UrunAdi,
                MevcutBolum = "Ekleme",
                Durum = x.Tamamlandi ? "Tamamlandı" : "Devam Ediyor",
                BaslamaTarihi = x.BaslamaTarihi,
                RenkKodu = x.Tamamlandi ? "success" : "warning"
            })
            .ToListAsync();

        var dazmalUrunler = await dazmalSet
            .Where(x => string.IsNullOrEmpty(search) ||
                       x.SiparisNo.Contains(search) ||
                       x.UrunAdi.Contains(search))
            .Select(x => new UrunDurumu
            {
                SiparisNo = x.SiparisNo,
                UrunAdi = x.UrunAdi,
                MevcutBolum = "Dazmal",
                Durum = x.Tamamlandi ? "Tamamlandı" : "Devam Ediyor",
                BaslamaTarihi = x.BaslamaTarihi,
                RenkKodu = x.Tamamlandi ? "success" : "info"
            })
            .ToListAsync();

        var kesmeUrunler = await kesmeSet
            .Where(x => string.IsNullOrEmpty(search) ||
                       x.SiparisNo.Contains(search) ||
                       x.UrunAdi.Contains(search))
            .Select(x => new UrunDurumu
            {
                SiparisNo = x.SiparisNo,
                UrunAdi = x.UrunAdi,
                MevcutBolum = "Kesme",
                Durum = x.Tamamlandi ? "Tamamlandı" : "Devam Ediyor",
                BaslamaTarihi = x.BaslamaTarihi,
                RenkKodu = x.Tamamlandi ? "success" : "primary"
            })
            .ToListAsync();

        var paketlemeUrunler = await paketlemeSet
            .Where(x => string.IsNullOrEmpty(search) ||
                       x.SiparisNo.Contains(search) ||
                       x.UrunAdi.Contains(search))
            .Select(x => new UrunDurumu
            {
                SiparisNo = x.SiparisNo,
                UrunAdi = x.UrunAdi,
                MevcutBolum = "Paketleme",
                Durum = "Devam Ediyor",
                BaslamaTarihi = DateTime.Now,
                RenkKodu = "secondary"
            })
            .ToListAsync();

        var hazirDokumaUrunler = await hazirDokumaSet
            .Where(x => string.IsNullOrEmpty(search) ||
                       x.SiparisNo.Contains(search) ||
                       x.UrunAdi.Contains(search))
            .Select(x => new UrunDurumu
            {
                SiparisNo = x.SiparisNo,
                UrunAdi = x.UrunAdi,
                MevcutBolum = "Hazır Dokuma",
                Durum = "Devam Ediyor",
                BaslamaTarihi = DateTime.Now,
                RenkKodu = "dark"
            })
            .ToListAsync();

        var hazirMatbaaUrunler = await hazirMatbaaSet
            .Where(x => string.IsNullOrEmpty(search) ||
                       x.SiparisNo.Contains(search) ||
                       x.UrunAdi.Contains(search))
            .Select(x => new UrunDurumu
            {
                SiparisNo = x.SiparisNo,
                UrunAdi = x.UrunAdi,
                MevcutBolum = "Hazır Matbaa",
                Durum = "Devam Ediyor",
                BaslamaTarihi = DateTime.Now,
                RenkKodu = "light"
            })
            .ToListAsync();

        // Tümünü birleştir
        dashboard.TumUrunDurumlari = makineSaatUrunler
            .Concat(eklemeUrunler)
            .Concat(dazmalUrunler)
            .Concat(kesmeUrunler)
            .Concat(paketlemeUrunler)
            .Concat(hazirDokumaUrunler)
            .Concat(hazirMatbaaUrunler)
            .OrderByDescending(x => x.BaslamaTarihi)
            .ToList();
    }

    // Database değiştirme endpoint'i
    [HttpPost]
    public IActionResult SwitchDatabase(string database)
    {
        return RedirectToAction("Index", new { database = database });
    }
}